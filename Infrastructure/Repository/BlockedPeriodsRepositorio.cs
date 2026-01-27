using AutoMapper;
using c___Api_Example.data;
using c___Api_Example.Domain.Models;
using c___Api_Example.Infrastructure.Repository.Interfaces;
using IbpvDtos;
using Microsoft.EntityFrameworkCore;

public class BlockedPeriodsRepositorio : IBlockedPeriodsRepositorio
{
    private readonly IbpvDataBaseContext _ibpvDataBaseContext;
    private readonly IMapper _mapper;
    private readonly ILogger<BlockedPeriodsRepositorio> _logger;

    public BlockedPeriodsRepositorio(IbpvDataBaseContext ibpvDataBaseContext, IMapper mapper, ILogger<BlockedPeriodsRepositorio> logger)
    {
        _ibpvDataBaseContext = ibpvDataBaseContext;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginetedResultDTO<BlockedPeriodModel>> GetPag()
    {
        // Default pagination: page 1, 10 items
        return await GetPag(1, 10,null);
    }

    public async Task<PaginetedResultDTO<BlockedPeriodModel>> GetPag(int pageNumber, int pageQuantity, DateTime? filterDate)
    {
        try
        {
            var query = _ibpvDataBaseContext.BlockedPeriod
                .Include(x => x.BlockedBy)
                .Include(x => x.PendingUnlocks)
                     .ThenInclude(pu => pu.BlockedUser)
                .AsQueryable();

            if (filterDate.HasValue)
            {
                query = query.Where(bp => bp.StartDate <= filterDate.Value && bp.EndDate >= filterDate.Value);
            }

            int totalCount = await query.CountAsync();

            var entities = await query
                .OrderByDescending(bp => bp.StartDate)
                .Skip((pageNumber - 1) * pageQuantity)
                .Take(pageQuantity)
                .ToListAsync();

            var models = entities;

            int totalPages = (int)Math.Ceiling((double)totalCount / pageQuantity);

            return new PaginetedResultDTO<BlockedPeriodModel>
            {
                items = models,
                pages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paginated blocked periods.");
            throw;
        }
    }

    public async Task<BlockedPeriodModel> GetById(int id)
    {
        try
        {
            var entity = await _ibpvDataBaseContext.BlockedPeriod
                .Include(bp => bp.PendingUnlocks)
                .FirstOrDefaultAsync(bp =>  bp.Id == id);

            if (entity == null)
                throw new Exception($"Blocked period with id {id} not found.");

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching blocked period by id {id}.");
            throw;
        }
    }

    public async Task<int> Add(BlockedPeriodModel item)
    {
        using var transaction = await _ibpvDataBaseContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            var entity = item;
            await _ibpvDataBaseContext.BlockedPeriod.AddAsync(entity);
            await _ibpvDataBaseContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return entity.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error adding blocked period.");
            throw;
        }
    }

    public async Task<int> Update( int id, BlockedPeriodModel item)
    {
        try
        {
            var entity = await _ibpvDataBaseContext.BlockedPeriod.FindAsync(id);
            if (entity == null)
                throw new Exception($"Blocked period with id {item.Id} not found for update.");

            _mapper.Map(item, entity);

            await _ibpvDataBaseContext.SaveChangesAsync();
            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating blocked period with id {item.Id}.");
            throw;
        }
    }

    public async Task<bool> Remove(int Id)
    {
        try
        {
            var entity = await _ibpvDataBaseContext.BlockedPeriod.FindAsync(Id);
            if (entity == null)
                throw new Exception($"Blocked period with id {Id} not found for removal.");

            _ibpvDataBaseContext.BlockedPeriod.Remove(entity);
            await _ibpvDataBaseContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing blocked period with id {Id}.");
            throw;
        }
    }

    public async Task<bool> HasOverlappingPeriod(BlockedPeriodModel item)
    {
        return await _ibpvDataBaseContext.BlockedPeriod.AnyAsync(
            p => p.IsBlocked && 
                 p.StartDate <= item.EndDate && p.EndDate >= item.StartDate &&
                 p.IsBlocked == true
        );
      
    }
    
    public async Task<bool> IsDateBlocked(DateTime date)
    {
        return await _ibpvDataBaseContext.BlockedPeriod.AnyAsync(
            p => p.IsBlocked &&
                 p.StartDate <= date &&
                 p.EndDate >= date 
        );
    }
    
 
}
