using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using c___Api_Example.Application.Services.ControllersServices;
using c___Api_Example.Infrastructure.Repository.Interfaces;
using IbpvDtos;
using c___Api_Example.Models;
using c___Api_Example.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace c___Api_Example.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GastoController : ControllerBase
    {
        readonly private IGastoRepositorio _gastoRepositorio;
        readonly private ExpenseService _expenseService;
        readonly private IMapper _mapper;
        private readonly IBlockedPeriodsRepositorio _BlockedPeriodsRepositorio;

        public GastoController(IGastoRepositorio gastoRepositorio,IMapper mapper,  IBlockedPeriodsRepositorio blockedPeriodsRepositorio,
            ExpenseService expenseService)
        {
            _gastoRepositorio = gastoRepositorio;
            _mapper = mapper;
            _BlockedPeriodsRepositorio = blockedPeriodsRepositorio;
            _expenseService = expenseService;
        }

        [HttpGet]
        /*ActionResult é um tipo de retorno para métodos de ação em controladores ASP.NET Core que encapsula o resultado de uma solicitação HTTP*/
        public async Task <ActionResult<PaginetedResultDTO<GastoPagDTO>>> GetPagGastos(int pageNumber, int pageQuantity, string? descricao = null, int? idCaixa = null, DateTime? initialDate = null, DateTime? finalDate = null)
        {
            var result = await _expenseService.GetPagExpenses(
                pageNumber,
                pageQuantity,
                descricao,
                idCaixa,
                initialDate,
                finalDate
            );
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseGetByIdDTO>> GetGastoById(int id)
        {
            var gasto = await _expenseService.GetExpenseByIdAsync(id);
            return Ok(gasto);
        }
        
        
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<int>> AddGasto(
            [FromForm] DtoGastoPost dtoGasto,
            [FromForm] List<IFormFile>? images)
        {
            int idGasto = await _expenseService.CreateExpenseAsync(dtoGasto, images);
            return Ok(idGasto);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteGasto(int id)
        {
            await _expenseService.DeleteExpenseAsync(id);
            return Ok();
        }
        
        [HttpPut("{id:int}")]
        public async Task<ActionResult<int>> UpdateGasto(int id, [FromForm] GastoPutDto dtoGasto,
            [FromForm] List<IFormFile>? images)
        {
            int updatedId = await _expenseService.UpdateExpenseAsync(id, dtoGasto, images);
            return Ok(updatedId);
        }
    }
}