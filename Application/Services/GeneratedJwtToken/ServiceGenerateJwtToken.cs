using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using c___Api_Example.Models;
using Microsoft.IdentityModel.Tokens;
using Npgsql.Internal;

namespace c___Api_Example.Services
{
    public class ServiceGenerateToken
    {
        private readonly string jwt_key;

        public ServiceGenerateToken(IConfiguration configuration)
        {
            string? key = configuration["AmbienteVar:jwt"];

            if(key is not null)
            {
                jwt_key = key;
            }
            else
            {
                throw new Exception("key jwt in ambiente var is null");
            }
        }

        public string GenerateToken(UsuarioModel user){
            var key = Encoding.ASCII.GetBytes(jwt_key); // transforma fazx um encondig da string para um array de bytes

            var securitykey = new SymmetricSecurityKey(key); //chave
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256Signature);

            //o que vai de informacao no token Claim
            var userClaims = new[]
            {
                new Claim(ClaimTypes.Sid , user.Id.ToString()),
                new Claim("status", user.Rule.ToString()),
                new Claim("alarmAuth", user.alarmAuth.ToString()),
              
            };
            
            //configuraçoes
            var tokenConfig = new JwtSecurityToken
            (
                // issuer: 
                // audience:
                claims:userClaims,
                expires:DateTime.Now.AddDays(1),
                signingCredentials:credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenConfig);
        }
        
        public string GenerateResetToken(int userId, string email)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),  
                new Claim("type", "reset"),                                 
                new Claim(JwtRegisteredClaimNames.Email, email)           
            };
    
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                // issuer: _config["Jwt:Issuer"],
                // audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}