
using AutoMapper;
using c___Api_Example.Application.Services.ControllersServices.interfaces;
using c___Api_Example.Infrastructure.Repository.Interfaces;
using IbpvDtos;
using c___Api_Example.Models;
using c___Api_Example.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace c___Api_Example.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class ContribuicaoController : ControllerBase
    {
        readonly private IContribuicaoRepositorio _contribuicaoRepositorio;
        readonly private IMapper _mapper;
        readonly private IBlockedPeriodsRepositorio  _blockedPeriodsRepositorio;
        private readonly ContributionService _contributionService;
        
        public ContribuicaoController(IContribuicaoRepositorio contribuicaoRepositorio, ContributionService contributionService, IMapper mapper,  IBlockedPeriodsRepositorio blockedPeriodsRepositorio)
        {
            _contribuicaoRepositorio = contribuicaoRepositorio;
            _mapper = mapper;
            _blockedPeriodsRepositorio = blockedPeriodsRepositorio;
            _contributionService = contributionService;
        }

        [HttpGet]
        /*ActionResult é um tipo de retorno para métodos de ação em controladores ASP.NET Core que encapsula o resultado de uma solicitação HTTP*/
        [HttpGet]
        public async Task<ActionResult<PaginetedResultDTO<ContribuicaoPagDTO>>> GetAll(
            int pageNumber, 
            int pageQuantity, 
            string? descricao = null, 
            int? idCaixa = null, 
            DateTime? initialDate = null, 
            DateTime? finalDate = null)
        {
            
            var result = await _contributionService.GetAllContributionsAsync(
                pageNumber, pageQuantity, descricao, idCaixa, initialDate, finalDate
            );

           
            return Ok(result);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<ContribuicaoGetByIdDTO>> GetById(int id)
        {
            var contribuicao = await _contributionService.GetContributionByIdAsync(id);
            return Ok(contribuicao);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<int>> CreateContribuicao(
            [FromForm] ContribuicaoPostDTO contribuicao,
            [FromForm] List<IFormFile>? images)
        {
            int id = await _contributionService.CreateContributionAsync(contribuicao,images);
            return Ok(id);
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _contributionService.DeleteContribuicaoAsync(id);
            return Ok();
        }
        
        [HttpPut("{id:int}")]
        public async Task<ActionResult<int>> Update(int id,  [FromForm] ContribuicaoPutDTO dto,
            [FromForm] List<IFormFile>? newImages)
        {
            int updatedId = await _contributionService.UpdateContributionAsync(id, dto, newImages);
            return Ok(updatedId);
        }

    }
}