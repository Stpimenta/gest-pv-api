using System.Security.Claims;
using AutoMapper;
using c___Api_Example.Application.Services.ControllersServices.interfaces;
using c___Api_Example.Domain.Models;
using c___Api_Example.Infrastructure.Repository.Interfaces;
using IbpvDtos;

namespace c___Api_Example.Application.Services.ControllersServices;

public class BlockedPeriodService : IBlockedPeriodService
{
    private readonly IBlockedPeriodsRepositorio _repo;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPendingUnlockRepositorio _pendingUnlockRepositorio;

    public BlockedPeriodService(IBlockedPeriodsRepositorio repo, IMapper mapper,  IHttpContextAccessor httpContextAccessor, IPendingUnlockRepositorio pendingUnlockRepositorio)
    {
        _repo = repo;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _pendingUnlockRepositorio = pendingUnlockRepositorio;
    }

    public async Task<PaginetedResultDTO<BlockedPeriodGetDto>> GetAll(int page, int quantity, DateTime? filterDate = null)
    {
        if(filterDate.HasValue)
            filterDate = DateTime.SpecifyKind(filterDate.Value, DateTimeKind.Utc);
        
        var result = await _repo.GetPag(page, quantity, filterDate);

        return new PaginetedResultDTO<BlockedPeriodGetDto>
        {
            items = _mapper.Map<List<BlockedPeriodGetDto>>(result.items),
            pages = result.pages,
        };
        
    }


    public async Task<BlockedPeriodGetDto?> GetById(int id)
    {
        return  _mapper.Map<BlockedPeriodGetDto>(await _repo.GetById(id));
    }
       

    public async Task<int> Create(BlockedPeriodsPostDTO dto)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Sid)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new ApplicationException("UserId claim is missing");
        }
        
        var model = _mapper.Map<BlockedPeriodModel>(dto);
        
        model.StartDate = DateTime.SpecifyKind(model.StartDate, DateTimeKind.Utc);
        model.EndDate   = DateTime.SpecifyKind(model.EndDate, DateTimeKind.Utc);
                 model.BlockedById = int.Parse(userId);
        model.IsBlocked = true;
        
        
        bool dataIsBlocked = await _repo.HasOverlappingPeriod(model);
        if (dataIsBlocked)
            throw new Exception("data is blocked");
        
        return await _repo.Add(model);
    }

    public async Task<int> Update(int id, BlockedPeriodModel model) => await _repo.Update(id, model);
    public async Task Delete(int id) => await _repo.Remove(id);
    
    public async Task<bool> IsDateBlocked(DateTime date)
    {
        return await _repo.IsDateBlocked(date);
    }

    public async Task ReblockPeriod(int blockPeriodId)
    {
        
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Sid)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new ApplicationException("UserId claim is missing");
        }
        
        var blockedPeriod = await _repo.GetById(blockPeriodId);
        if (blockedPeriod == null)
        { 
            throw new Exception("blockedPeriod not found");
        }
        
        blockedPeriod.IsBlocked = true;
        blockedPeriod.BlockedById = int.Parse(userId);
        
        await _repo.Update(blockPeriodId, blockedPeriod);
        
        var pendingUnlocks = blockedPeriod.PendingUnlocks?.Where(p => p.IsActive == true).ToList();

        if (pendingUnlocks != null)
        {
            foreach (var pendingUnlock in pendingUnlocks)
            {
                pendingUnlock.IsActive = false;
                await _pendingUnlockRepositorio.Update(pendingUnlock.Id, pendingUnlock);
            }
        }
        
    }
}
