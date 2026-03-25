using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BCrypt.Net;
using IbpvDtos;
using c___Api_Example.data;
using c___Api_Example.Models;
using c___Api_Example.repository;
using c___Api_Example.repository.Interfaces;

namespace c___Api_Example.Application.Services.UserCryptography
{
    public class ServiceUserPassCryptography : IServiceUserPassCryptography
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        public ServiceUserPassCryptography(IUsuarioRepositorio usuarioRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
        }

        public async Task<(bool,int)> comparePassUserLogin(LoginDTO credentials)
        {   
            UsuarioModel? user = await _usuarioRepositorio.GetUserByGmail(credentials.gmail);
            if(user is null)
            {
                return (false,0);
            }
            bool compare = BCrypt.Net.BCrypt.Verify(credentials.senha,user.Senha);

            if(compare)
            {
                return (compare,user.Id);
            }

            return (false,0);
        }

        public string encryptPassUser(string pass)
        {
            return BCrypt.Net.BCrypt.HashPassword(pass);
        }

        public async Task<int> updateUserPassword(int id, string pass)
        {
            string encryptPass = encryptPassUser(pass);
            int userid = await _usuarioRepositorio.UpdatePassword(id, encryptPass);
            return userid;
        }
        
        public string GenerateTemporaryPassword(int length = 10)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";

            var bytes = RandomNumberGenerator.GetBytes(length);
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[bytes[i] % chars.Length];
            }

            return new string(result);
        }
        
    }
}