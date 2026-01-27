using c___Api_Example.Domain.Models;
using IbpvDtos;

namespace c___Api_Example.Infrastructure.Repository.Interfaces;

public interface IBlockedPeriodsRepositorio
{
    Task<PaginetedResultDTO<BlockedPeriodModel>> GetPag(int pageNumber, int pageQuantity, DateTime? filterDate);
    
    Task<BlockedPeriodModel> GetById(int id);
    Task<int> Add(BlockedPeriodModel item); 
    Task<int> Update(int id, BlockedPeriodModel item);
    Task<bool> Remove(int Id);
    Task<bool> HasOverlappingPeriod(BlockedPeriodModel item); 
    
    Task<bool> IsDateBlocked(DateTime date); 
    
} 