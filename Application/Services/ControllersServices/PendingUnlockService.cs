using System.Security.Claims;
using AutoMapper;
using c___Api_Example.Application.Services.ControllersServices.interfaces;
using c___Api_Example.Domain.Models;
using c___Api_Example.Infrastructure.Repository.Interfaces;
using IbpvDtos;

namespace c___Api_Example.Application.Services.ControllersServices;

public class PendingUnlockService : IPendingUnlockService
{
    private readonly IPendingUnlockRepositorio _repo;
    private readonly IBlockedPeriodsRepositorio _blockedRepo;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public PendingUnlockService(IPendingUnlockRepositorio repo , IMapper mapper, IBlockedPeriodsRepositorio blockedRepo, IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _mapper = mapper;
        _blockedRepo = blockedRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PaginetedResultDTO<PendingUnlockModel>> GetAll(int page, int quantity) =>
        await _repo.GetPag(page, quantity);

    public async Task<PendingUnlockModel?> GetById(int id) =>
        await _repo.GetById(id);

    public async Task<int> Create(PendingUnlockPostDto dto)
    {
        var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
        if (userIdClaim is null)
        {
            throw new ApplicationException("No user claim found");
        }
        
            
        var model = _mapper.Map<PendingUnlockModel>(dto);
        model.BlockUserId = int.Parse(userIdClaim);
        model.IsActive = true;

        var existingCount = await _repo.CountPendingUnlocksForBlockedPeriod(model.BlockPeriodId);
        if (existingCount >= 2)
        {
            throw new Exception($"Block period is  alredy unlocked");
        }
        
        var existingUnlockForUser = await _repo.ExistingPendingUnlockForUser(userId:model.BlockUserId, blockedPeriodId:model.BlockPeriodId);
        if (existingUnlockForUser)
        {
            throw new Exception($"You  alredy unlocked this period");
        }
        
        if (existingCount == 1)
        {
            var blockedPeriod = await _blockedRepo.GetById(model.BlockPeriodId);
            if (blockedPeriod != null && blockedPeriod.IsBlocked)
            {
                blockedPeriod.IsBlocked = false;
                await _blockedRepo.Update(blockedPeriod.Id, blockedPeriod);
            }
        }
        
        return await _repo.Add(model);                                                                                                          
    }

    public async Task<int> Update(int id, PendingUnlockModel model) =>
        await _repo.Update(id, model);

    public async Task Delete(int id) =>
        await _repo.Remove(id);
}