using AutoMapper;
using c___Api_Example.Application.Services.ControllersServices.interfaces;
using IbpvDtos;
using c___Api_Example.Domain.Models;
using c___Api_Example.Infrastructure.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace c___Api_Example.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PendingUnlockController : ControllerBase
    {
        private readonly IPendingUnlockService _pendingUnlockService;

        public PendingUnlockController(IPendingUnlockService pendingUnlockService)
        {
            _pendingUnlockService = pendingUnlockService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginetedResultDTO<PendingUnlockGetDto>>> GetAll(
            int pageNumber = 1,
            int pageQuantity = 10)
        {
            var pagedResult = await _pendingUnlockService.GetAll(pageNumber, pageQuantity);
            return Ok(pagedResult);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PendingUnlockModel>> GetById(int id)
        {
            var pendingUnlock = await _pendingUnlockService.GetById(id);
            if (pendingUnlock == null)
                return NotFound($"Pending unlock with id {id} not found.");

            return Ok(pendingUnlock);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] PendingUnlockPostDto pendingUnlock)
        {
            var id = await _pendingUnlockService.Create(pendingUnlock);
            return Ok(id);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<int>> Update(int id, [FromBody] PendingUnlockModel pendingUnlock)
        {
            var updatedId = await _pendingUnlockService.Update(id, pendingUnlock);
            return Ok(updatedId);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _pendingUnlockService.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}