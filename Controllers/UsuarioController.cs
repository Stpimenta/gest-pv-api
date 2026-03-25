using IbpvDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using c___Api_Example.Application.Services;
using c___Api_Example.Application.Services.ControllersServices;

namespace c___Api_Example.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly UserService _usuarioService;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(
            UserService usuarioService,
            ILogger<UsuarioController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PaginetedResultDTO<UsuarioPagDTO>>> GetAllUser(
            int page, int itensQuantity, string? nome, string? token)
        {
            var result = await _usuarioService.GetPaginated(page, itensQuantity, nome, token);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioGetByIdDTO>> GetUserById(int id)
        {
            var result = await _usuarioService.GetById(id);

            if (result is null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<int>> CreateUser(
            [FromForm] UsuarioPostDTO user)
        {
            var id = await _usuarioService.Create(user);
            return Ok(id);
        }

        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateByid(
            int id,
            [FromForm] UsuarioPutDTO user)
        {
            var result = await _usuarioService.Update(id, user);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<bool>> DeleteById(int id)
        {
            var result = await _usuarioService.Delete(id);
            return Ok(result);
        }
    }
}