using c___Api_Example.Domain.Models;
using IbpvDtos;

namespace c___Api_Example.Infrastructure.Repository.Interfaces;

public interface IPendingUnlockRepositorio
{
    Task<PaginetedResultDTO<PendingUnlockModel>> GetPag(int pageNumber, int pageQuantity);
    Task<PendingUnlockModel> GetById(int id);
    Task<int> Add(PendingUnlockModel item);
    Task<int> Update(int Id, PendingUnlockModel item);
    Task<int> Remove(int Id);
    
    Task<int> CountPendingUnlocksForBlockedPeriod(int blockedPeriodId);
    
    Task<bool> ExistingPendingUnlockForUser(int blockedPeriodId, int userId);

}