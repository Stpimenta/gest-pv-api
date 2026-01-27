
using System.Security.Claims;
using IbpvDtos;
using c___Api_Example.Application.Services.UserCryptography;
using c___Api_Example.repository.Interfaces;
using c___Api_Example.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace c___Api_Example.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        IServiceUserPassCryptography _serviceUserPassCryptography;
        ServiceGenerateToken serviceGenerateToken;

        //password from firstuser appsettings
        string? rootuser;
        string? rootpass;

        private IUsuarioRepositorio _repoUser;
        public LoginController(IServiceUserPassCryptography serviceUserPassCryptography, ServiceGenerateToken serviceGenerateToken, IConfiguration configuration, IUsuarioRepositorio repoUser)
        {
            _serviceUserPassCryptography = serviceUserPassCryptography;
            this.serviceGenerateToken = serviceGenerateToken;
            rootuser = configuration["AmbienteVar:user"];
            rootpass = configuration["AmbienteVar:password"];
            _repoUser = repoUser;
        }

        [HttpPost]
        public async Task<ActionResult<RespondeLoginDto>> Login ([FromBody] LoginDTO login)
        {
            // if(rootpass is not null && rootuser is not null)
            // {
            //     if(login.gmail==rootuser && login.senha == rootpass)
            //     {
            //         string Token = serviceGenerateToken.GenerateToken();
            //         return Ok( new RespondeLoginDto {
            //             status = true,
            //             jwtToken = Token
            //         });
            //     }
            // }
            //     

            (bool compare, int id) serviceLogin = await _serviceUserPassCryptography.comparePassUserLogin(login);
            
            if(serviceLogin.compare){
                var user = await _repoUser.GetUserById(serviceLogin.id);
                string Token = serviceGenerateToken.GenerateToken(user);
                return Ok( new RespondeLoginDto {
                    status = serviceLogin.compare,
                    jwtToken = Token
                });
            }
            
            return Unauthorized(new RespondeLoginDto {
                    status = serviceLogin.compare,
                    jwtToken = ""
                });
        }
        
        [Authorize]
        [HttpPut("UpdatePass/")]
        public async Task<ActionResult<int>> UpdatePassword([FromBody]LoginUpdatePassDTO user)
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id== null)
                return Unauthorized();

            int userId = int.Parse(id);
            int updatedId = await _serviceUserPassCryptography.updateUserPassword(userId,user.newPassword!);
            
            return Ok(updatedId);
        }

        [Authorize]
        [HttpPut("forgot-password{email}")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _repoUser.GetUserByGmail(email);

            if (user == null)
            {
                return BadRequest(new { status = false, message = "Email not found" });
            }

            var resetToken = serviceGenerateToken.GenerateResetToken(user.Id, email);
            
            return Ok(new { status = true, token = resetToken });
        }   
    }
}