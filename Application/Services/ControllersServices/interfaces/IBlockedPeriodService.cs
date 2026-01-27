using c___Api_Example.Domain.Models;
using IbpvDtos;

namespace c___Api_Example.Application.Services.ControllersServices.interfaces;

public interface IBlockedPeriodService
{
    Task<PaginetedResultDTO<BlockedPeriodGetDto>> GetAll(int page, int quantity,DateTime? filterDate = null);
    Task<BlockedPeriodGetDto?> GetById(int id);
    Task<int> Create(BlockedPeriodsPostDTO model);
    Task<int> Update(int id, BlockedPeriodModel model);
    Task Delete(int id);
    
    Task<bool> IsDateBlocked(DateTime date);

    public Task ReblockPeriod(int blockPeriodId);
}
