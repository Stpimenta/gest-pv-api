using System.Runtime.CompilerServices;
using AutoMapper;
using c___Api_Example.data;
using c___Api_Example.Domain.Models;
using c___Api_Example.Infrastructure.Repository.Interfaces;
using IbpvDtos;
using Microsoft.EntityFrameworkCore;

public class  PendingUnlockRepositorio : IPendingUnlockRepositorio
{
    private readonly IbpvDataBaseContext _ibpvDataBaseContext;
    private readonly IMapper _mapper;
    private readonly ILogger<PendingUnlockRepositorio> _logger;

    public PendingUnlockRepositorio(IbpvDataBaseContext ibpvDataBaseContext, IMapper mapper, ILogger<PendingUnlockRepositorio> logger)
    {
        _ibpvDataBaseContext = ibpvDataBaseContext;
        _mapper =  mapper;
        _logger = logger;
    }
    
    public async Task<PaginetedResultDTO<PendingUnlockModel>> GetPag()
    {
        return await GetPag(1, 10);
    }

    public async Task<PendingUnlockModel> GetById(int id)
    {
        try
        {
            var entity = await _ibpvDataBaseContext.PendingUnlock.FindAsync(id);
            if (entity == null)
                throw new Exception($"Pending unlock with id {id} not found.");

            return entity; // se a entidade for o model direto
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching pending unlock by id {id}.");
            throw;
        }
    }
    
    public async Task<int> CountPendingUnlocksForBlockedPeriod(int blockedPeriodId)
    {
      
            return await _ibpvDataBaseContext.PendingUnlock
                .Where(pu => pu.BlockPeriodId == blockedPeriodId && pu.IsActive)
                .CountAsync();
    }

    public async Task<bool> ExistingPendingUnlockForUser(int blockedPeriodId, int userId)
    {
        return await _ibpvDataBaseContext.PendingUnlock.AnyAsync(
            pu => pu.BlockPeriodId == blockedPeriodId &&
                  pu.BlockUserId == userId &&
                  pu.IsActive);
    }

    public async Task<PaginetedResultDTO<PendingUnlockModel>> GetPag(int pageNumber, int pageQuantity)
    {
        try
        {
            var query = _ibpvDataBaseContext.PendingUnlock.AsQueryable();

            int totalCount = await query.CountAsync();

            var entities = await query
                .OrderBy(pu => pu.Id)
                .Skip((pageNumber - 1) * pageQuantity)
                .Take(pageQuantity)
                .ToListAsync();

            var models = _mapper.Map<List<PendingUnlockModel>>(entities);

            int totalPages = (int)Math.Ceiling((double)totalCount / pageQuantity);

            return new PaginetedResultDTO<PendingUnlockModel>
            {
                items = models,
                pages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paginated pending unlocks.");
            throw;
        }
    }
    

    public async Task<int> Add(PendingUnlockModel item)
    {
        try
        {
            await _ibpvDataBaseContext.PendingUnlock.AddAsync(item);
            await _ibpvDataBaseContext.SaveChangesAsync();
            return item.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding pending unlock.");
            throw;
        }
    }


    public async Task<int> Update(int Id, PendingUnlockModel pendingUnlockUpdate)
    {
        try
        {
            var existingEntity = await _ibpvDataBaseContext.PendingUnlock.FindAsync(Id);
            if (existingEntity == null)
                throw new Exception("Pending unlock not found");

            _mapper.Map(pendingUnlockUpdate, existingEntity);

            await _ibpvDataBaseContext.SaveChangesAsync();

            return existingEntity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating pending unlock with id {Id}.");
            throw;
        }
    }

    public async Task<int> Remove(int Id)
    {
        try
        {
            var entity = await _ibpvDataBaseContext.PendingUnlock.FindAsync(Id);
            if (entity == null)
                throw new Exception("Pending unlock not found");

            _ibpvDataBaseContext.PendingUnlock.Remove(entity);
            await _ibpvDataBaseContext.SaveChangesAsync();

            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing pending unlock with id {Id}.");
            throw;
        }
    }
}
