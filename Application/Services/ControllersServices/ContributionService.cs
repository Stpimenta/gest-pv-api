using AutoMapper;
using c___Api_Example.Application.Services.Gdrive;
using c___Api_Example.Domain.Models;
using c___Api_Example.Models;
using c___Api_Example.repository.Interfaces;
using c___Api_Example.Repository.Interfaces;
using IbpvDtos;

namespace c___Api_Example.Application.Services.ControllersServices.interfaces;

public class ContributionService
{
    private readonly IContribuicaoRepositorio _contribuicaoRepositorio;
    private readonly IUsuarioRepositorio _userRepositorio;
    private readonly IMapper _mapper;
    private readonly IBlockedPeriodService _blockedPeriodService;
    private readonly MinioService _minioService;

    public ContributionService(IContribuicaoRepositorio contribuicaoRepositorio, IMapper mapper,  
        IBlockedPeriodService blockedPeriodService,  MinioService minioService,  IUsuarioRepositorio userRepositorio)
    {
        _contribuicaoRepositorio = contribuicaoRepositorio;
        _mapper = mapper;
        _blockedPeriodService = blockedPeriodService;
        _minioService = minioService;
        _userRepositorio = userRepositorio;
    }
    
    public async Task<PaginetedResultDTO<ContribuicaoPagDTO>> GetAllContributionsAsync(
        int pageNumber, 
        int pageQuantity, 
        string? descricao = null, 
        int? idCaixa = null, 
        DateTime? initialDate = null, 
        DateTime? finalDate = null)
    {
        // UTC dates
        if (initialDate.HasValue)
            initialDate = DateTime.SpecifyKind(initialDate.Value, DateTimeKind.Utc);

        if (finalDate.HasValue)
            finalDate = DateTime.SpecifyKind(finalDate.Value, DateTimeKind.Utc);

        //get
        var paginatedModel = await _contribuicaoRepositorio.GetPagContribuicoes(
            pageNumber, pageQuantity, descricao, idCaixa, initialDate, finalDate
        );

        //map dto
        var itemsDto = _mapper.Map<List<ContribuicaoPagDTO>>(paginatedModel.items);

        return new PaginetedResultDTO<ContribuicaoPagDTO>
        {
            items = itemsDto,
            pages = paginatedModel.pages
        };
    }
    
    public async Task<ContribuicaoGetByIdDTO> GetContributionByIdAsync(int id)
    {
       var contribution = await _contribuicaoRepositorio.GetContribuicaoById(id);
       if(contribution == null)
           throw new Exception("Contribuição não encontrada");
       
       var dto = new ContribuicaoGetByIdDTO
       {
           Id = contribution.Id,
           Valor = contribution.Valor,
           Descricao = contribution.Descricao,
           Data = contribution.Data,
           UrlEnvelope = contribution.UrlEnvelope,
           IdCaixa = contribution.IdCaixa,
           IdMembro = contribution.IdMembro,
           TokenMembro = null,
           Images = new List<ContribuicaoImageDTO>()
       };
       
       if(contribution.IdMembro.HasValue)
       {
           var member = await _userRepositorio.GetUserById(contribution.IdMembro.Value);
           dto.TokenMembro = member.TokenContribuicao;
       }
       
       foreach (var image in contribution.Images)
       {
           var presignedUrl = await _minioService.GetPresignedUrlAsync(
               bucketName: "financial",
               objectName: image.Url,
               expiresInSeconds: 600
           );

           dto.Images.Add(new ContribuicaoImageDTO
           {
               Id = image.Id,
               Url = image.Url,
               PresignedUrl = presignedUrl
           });
       }

       return dto;
    }
    
    public async Task<int> CreateContributionAsync(ContribuicaoPostDTO dto, List<IFormFile>? images)
    {
        if (!dto.Data.HasValue)
            throw new ArgumentException("Data não pode ser nula");
        
        dto.Data = DateTime.SpecifyKind(dto.Data.Value, DateTimeKind.Utc);
    
        if (await _blockedPeriodService.IsDateBlocked(dto.Data.Value))
            throw new InvalidOperationException("This date is in a blocked period.");
        ContribuicaoModel contribution = new ContribuicaoModel
        {
            IdCaixa = dto!.IdCaixa.Value,
            Data =  dto.Data,
            Descricao = dto.Descricao,
            IdMembro =  dto.IdMembro,
            Valor =  dto.Valor,
            Images = new List<ContributionImageModel>()
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

                contribution.Images.Add(new ContributionImageModel
                {
                    Url = objectName,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return await _contribuicaoRepositorio.AddContribuicao(contribution);
    }
    
    public async Task DeleteContribuicaoAsync(int id)
    {
        var contribuicao = await _contribuicaoRepositorio.GetContribuicaoById(id);
        if (contribuicao == null)
            throw new KeyNotFoundException("Contribuição não encontrada.");

        if (contribuicao.Data.HasValue && await _blockedPeriodService.IsDateBlocked(contribuicao.Data.Value))
            throw new InvalidOperationException("Cannot delete a contribution in a blocked period.");
        
        if (contribuicao.Images is not null)
        {
            foreach (var img in contribuicao.Images)
            {
                await _minioService.DeleteAsync(
                    bucket: "financial",
                    objectName: img.Url
                );
            }
        }

        bool deleted = await _contribuicaoRepositorio.DeleteContribuicao(id);
        if (!deleted)
            throw new Exception("Erro ao deletar contribuição.");
    }
    
    
  public async Task<int> UpdateContributionAsync(
    int id,
    ContribuicaoPutDTO dto,
    List<IFormFile>? newImages)
{
    var contribution = await _contribuicaoRepositorio.GetContribuicaoById(id);

    if (contribution == null)
        throw new KeyNotFoundException("Contribution not found");

    if (contribution.Data.HasValue &&
        await _blockedPeriodService.IsDateBlocked(contribution.Data.Value))
        throw new InvalidOperationException("Cannot update a contribution in a blocked period");

    var model = new ContribuicaoModel
    {
        Id = id,
        Valor = dto.Valor,
        Descricao = dto.Descricao,
        Data = DateTime.SpecifyKind(dto.Data!.Value, DateTimeKind.Utc),
        IdCaixa = dto.IdCaixa!.Value,
        IdMembro = dto.IdMembro,
        UrlEnvelope = dto.UrlEnvelope,
        Images = new List<ContributionImageModel>()
    };

    // keep only images whose Ids are in KeepImageIds
    var keepIds = dto.KeepImageIds.ToHashSet();

    foreach (var img in contribution.Images)
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

            model.Images.Add(new ContributionImageModel
            {
                Url = objectName,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    return await _contribuicaoRepositorio.UpdateContribuicao(id, model);
}

    
        
    
    
}