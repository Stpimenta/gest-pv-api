using c___Api_Example.Domain.Models;
using IbpvDtos;

namespace c___Api_Example.Application.Services.ControllersServices.interfaces;

public interface IPendingUnlockService
{
    Task<PaginetedResultDTO<PendingUnlockModel>> GetAll(int page, int quantity);
    Task<PendingUnlockModel?> GetById(int id);
    Task<int> Create(PendingUnlockPostDto model);
    Task<int> Update(int id, PendingUnlockModel model);
    Task Delete(int id);
}
