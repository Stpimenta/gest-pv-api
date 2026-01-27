using AutoMapper;
using c___Api_Example.Application.Services.ControllersServices.interfaces;
using c___Api_Example.Application.Services.Gdrive;
using c___Api_Example.Domain.Models;
using c___Api_Example.Infrastructure.Repository.Interfaces;
using c___Api_Example.Models;
using c___Api_Example.Repository.Interfaces;
using IbpvDtos;

namespace c___Api_Example.Application.Services.ControllersServices;

public class ExpenseService
{
    private readonly IGastoRepositorio _expenseRepository;
    private readonly IMapper _mapper;
    private readonly IBlockedPeriodService _blockedPeriodsService;
    private readonly MinioService _minioService;

    public ExpenseService(IGastoRepositorio expenseRepository, IMapper mapper, IBlockedPeriodService blockedPeriodsService, 
        MinioService minioService)
    {
        _expenseRepository = expenseRepository;
        _mapper = mapper;
        _blockedPeriodsService = blockedPeriodsService;
        _minioService = minioService;
    }
    
    public async Task<PaginetedResultDTO<GastoPagDTO>> GetPagExpenses(
        int pageNumber,
        int pageQuantity,
        string? descricao = null,
        int? idCaixa = null,
        DateTime? initialDate = null,
        DateTime? finalDate = null)
    {
        if (initialDate is not null)
        {
            initialDate = DateTime
                .SpecifyKind(initialDate.Value, DateTimeKind.Utc)
                .Date;
        }

        if (finalDate is not null)
        {
            finalDate = DateTime
                .SpecifyKind(finalDate.Value, DateTimeKind.Utc)
                .Date;
        }

        PaginetedResultDTO<GastoModel> gastosModel =
            await _expenseRepository.GetPageGastos(
                pageNumber,
                pageQuantity,
                descricao,
                idCaixa,
                initialDate,
                finalDate
            );

        var gastosDTO = _mapper.Map<List<GastoPagDTO>>(gastosModel.items);

        return new PaginetedResultDTO<GastoPagDTO>
        {
            items = gastosDTO,
            pages = gastosModel.pages
        };
    }


    public async Task<ExpenseGetByIdDTO> GetExpenseByIdAsync(int id)
    {
        var expense = await _expenseRepository.GetGastoById(id);

        if (expense is null)
            throw new KeyNotFoundException($"Expense {id} not found");

        var dto = new ExpenseGetByIdDTO
        {
            Id = expense.Id,
            Valor = expense.Valor,
            Descricao = expense.Descricao,
            Data = expense.Data,
            UrlComprovante = expense.UrlComprovante,
            NumeroFiscal = expense.NumeroFiscal,
            IdCaixa = expense.IdCaixa,
            Images = new List<ExpenseImageDTO>()
        };

        foreach (var image in expense.Images)
        {
            var presignedUrl = await _minioService.GetPresignedUrlAsync(
                bucketName: "financial",
                objectName: image.Url,
                expiresInSeconds: 600
            );

            dto.Images.Add(new ExpenseImageDTO()
            {
                Id = image.Id,
                Url = image.Url,
                PresignedUrl = presignedUrl
            });
        }

        return dto;
    }
    
    public async Task<int> CreateExpenseAsync(
        DtoGastoPost expenseDto,
        List<IFormFile>? images)
    {
        if (!expenseDto.Data.HasValue)
            throw new ArgumentException("Data é obrigatória");

        expenseDto.Data = DateTime.SpecifyKind(
            expenseDto.Data.Value,
            DateTimeKind.Utc
        );

        if (await _blockedPeriodsService.IsDateBlocked(expenseDto.Data.Value))
            throw new InvalidOperationException("This date is in a blocked period.");

        var expense = new GastoModel
        {
            IdCaixa = expenseDto.IdCaixa!.Value,
            Data = expenseDto.Data.Value,
            Descricao = expenseDto.Descricao,
            Valor = expenseDto.Valor,
            Images = new List<ExpenseImageModel>()
        };

        if (images is not null)
        {
            foreach (var image in images)
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                ms.Position = 0;

                var objectName =
                    $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";

                await _minioService.UploadAsync(
                    bucket: "financial",
                    stream: ms,
                    objectName: objectName
                );

                expense.Images.Add(new ExpenseImageModel
                {
                    Url = objectName,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return await _expenseRepository.Addgasto(expense);
    }
    
    public async Task<int> UpdateExpenseAsync(
        int id,
        GastoPutDto dto,
        List<IFormFile>? newImages)
    {
        var expense = await _expenseRepository.GetGastoById(id);

        if (expense is null)
            throw new KeyNotFoundException("Expense not found");

        if (await _blockedPeriodsService.IsDateBlocked(expense.Data))
            throw new InvalidOperationException("Cannot update an expense in a blocked period");

        var model = new GastoModel
        {
            Id = id,
            Valor = dto.Valor,
            Descricao = dto.Descricao,
            Data = DateTime.SpecifyKind(dto.Data!.Value, DateTimeKind.Utc),
            IdCaixa = dto.IdCaixa!.Value,
            Images = new List<ExpenseImageModel>()
        };

        // keep only images whose Ids are in KeepImageIds
        var keepIds = dto.KeepImageIds.ToHashSet();

        foreach (var img in expense.Images)
        {
            if (keepIds.Contains(img.Id))
            {
                model.Images.Add(img); // keep existing
            }
            else
            {
                // delete from MinIO
                await _minioService.DeleteAsync(
                    bucket: "financial",
                    objectName: img.Url
                );
            }
        }

        // add new images
        if (newImages is not null)
        {
            foreach (var file in newImages)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;

                var objectName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                await _minioService.UploadAsync(
                    bucket: "financial",
                    stream: ms,
                    objectName: objectName
                );

                model.Images.Add(new ExpenseImageModel
                {
                    Url = objectName,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return await _expenseRepository.UpdateGasto(id, model);
    }
    
    public async Task DeleteExpenseAsync(int id)
    {
        var expense = await _expenseRepository.GetGastoById(id);

        if (expense is null)
            throw new KeyNotFoundException("Expense not found");

        if (await _blockedPeriodsService.IsDateBlocked(expense.Data))
            throw new InvalidOperationException("Cannot delete an expense in a blocked period");

        // delete associated images from MinIO
        if (expense.Images is not null)
        {
            foreach (var img in expense.Images)
            {
                await _minioService.DeleteAsync(
                    bucket: "financial",
                    objectName: img.Url
                );
            }
        }

        bool deleted = await _expenseRepository.DeleteGasto(id);

        if (!deleted)
            throw new Exception("Error deleting expense");
    }
    
    
    
}