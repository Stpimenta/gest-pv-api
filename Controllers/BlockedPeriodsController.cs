using AutoMapper;
using c___Api_Example.Application.Services.ControllersServices.interfaces;
using IbpvDtos;
using c___Api_Example.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace c___Api_Example.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BlockedPeriodsController : ControllerBase
    {
        private readonly IBlockedPeriodService _blockedPeriodService;

        public BlockedPeriodsController(IBlockedPeriodService blockedPeriodService)
        {
            _blockedPeriodService = blockedPeriodService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginetedResultDTO<BlockedPeriodGetDto>>> GetAll(int pageNumber = 1, int pageQuantity = 10, DateTime? filterDate = null)
        {
            var pagedResult = await _blockedPeriodService.GetAll(pageNumber, pageQuantity, filterDate);
            return Ok(pagedResult);
        }
        
        
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BlockedPeriodGetDto>> GetById(int id)
        {
            var blockedPeriod = await _blockedPeriodService.GetById(id);
            if (blockedPeriod == null)
                return NotFound($"Blocked period with id {id} not found.");
            return Ok(blockedPeriod);
        }

        [HttpGet("is-date-blocked")]

        public async Task<ActionResult<bool>> IsDateBlocked([FromQuery] DateTime date)
        {
            
            var utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            bool isBlocked = await _blockedPeriodService.IsDateBlocked(utcDate);
            return Ok(isBlocked);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] BlockedPeriodsPostDTO blockedPeriod)
        {
            var id = await _blockedPeriodService.Create(blockedPeriod);
            return Ok(id);
        }
        
        [HttpPut("reblock/{blockPeriodId:int}")]
        public async Task<IActionResult> ReblockPeriod(int blockPeriodId)
        {
            await _blockedPeriodService.ReblockPeriod(blockPeriodId);
            return Ok($"Blocked period {blockPeriodId} has been reblocked successfully.");
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<int>> Update(int id, [FromBody] BlockedPeriodModel blockedPeriod)
        {
            var updatedId = await _blockedPeriodService.Update(id, blockedPeriod);
            return Ok(updatedId);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _blockedPeriodService.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
