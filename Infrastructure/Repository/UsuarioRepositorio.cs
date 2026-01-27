using AutoMapper;
using IbpvDtos;
using c___Api_Example.Application.Services.GeneratedUserToken;
using c___Api_Example.data;
using c___Api_Example.Models;
using c___Api_Example.repository.Interfaces;
using IbpvDtos.Enums;
using Microsoft.EntityFrameworkCore;

namespace c___Api_Example.repository
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        /* injeçao de de[endencia */
        private readonly IbpvDataBaseContext _dataBaseContext;
        private readonly IServiceGeneratedUserToken _userToken;
        private readonly IMapper _mapper;
       public UsuarioRepositorio(IbpvDataBaseContext ibpvDataBaseContext, IServiceGeneratedUserToken userToken, IMapper mapper)
        {   
            _dataBaseContext = ibpvDataBaseContext;
            _mapper = mapper;
            _userToken  = userToken;
        }

        public async Task<int> AddUser(UsuarioModel user)
        {
            if(user.TokenContribuicao is null){
                user.TokenContribuicao = _userToken.generateUserToken36(5);
            }

            int tentativas = 0;
            int maxtentativas = 10;
            while(await _dataBaseContext.Usuarios.AnyAsync(u => u.TokenContribuicao == user.TokenContribuicao))
            {
                tentativas ++;
                user.TokenContribuicao = _userToken.generateUserToken36(5);
                if(tentativas>=maxtentativas)
                {
                    throw new Exception("nao foi possivel gerar um novo token");
                }
            }

            await _dataBaseContext.Usuarios.AddAsync(user);
            await _dataBaseContext.SaveChangesAsync();
            return user.Id;
        }

        public async Task<bool> DeleteUserById(int id)
        {
            UsuarioModel? user = await GetUserById(id);
            if(user == null)
            {
                throw new Exception($"usuario para id: {id} não existe");
            }
            _dataBaseContext.Usuarios.Remove(user);
            await _dataBaseContext.SaveChangesAsync();
            return true;
        }

        public async Task<PaginetedResultDTO<UsuarioPagDTO>> GetPagUsers(int pageNumber, int pageQuantity, string? nome, string? token) 
        {
            //forma rustica de implementar dtos tem como fazer com mapping
            var query = _dataBaseContext.Usuarios.AsQueryable();

            if(nome is not null)
            {
                query = query.Where(u => u.Nome!.ToLower().Trim().Contains(nome.ToLower().Trim()));
            }

            if(token is not null)
            {
                 query = query.Where(u => u.TokenContribuicao!.ToLower().Trim() == token.ToLower().Trim());
            }

            List<UsuarioPagDTO> usuarios = await query
            .OrderBy(u => u.Nome)
            .Skip((pageNumber - 1) * pageQuantity )
            .Take(pageQuantity)
            .Select((u) => new UsuarioPagDTO
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Data_nascimento = (DateTime)u.Data_nascimento,
                TokenContribuicao = u.TokenContribuicao,
                Role = (EnumRole)u.Rule!  // Forçando a conversão (pode lançar se for null)

            }).ToListAsync();

            int pagesTotal = (int) Math.Ceiling((double) await query.CountAsync() / pageQuantity);
            return new PaginetedResultDTO<UsuarioPagDTO> {
                items = usuarios,
                pages = pagesTotal
            };

        }


        public async Task<UsuarioModel?> GetUserById(int id)
        {
            var user = await _dataBaseContext.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<bool> UpdateUser(int id, UsuarioPutDTO  userUpdate)
        {
            var userExisting = await GetUserById(id);
            if (userExisting == null)
                throw new Exception($"Usuário com id {id} não existe.");

            // Atualiza os campos usando o AutoMapper
            _mapper.Map(userUpdate, userExisting);

            await _dataBaseContext.SaveChangesAsync();
            return true;
        }
        
        public async Task<UsuarioModel?> GetUserByGmail(string gmail)
        {
            UsuarioModel? user = await _dataBaseContext.Usuarios.FirstOrDefaultAsync(u => u.Email == gmail);
            return user;
        }

        public async Task<int> UpdatePassword(int id, string password)
        {
            UsuarioModel? user;

            user = await GetUserById(id);
            
            if (user == null)
                throw new Exception($"usuario {id} não existe");
            
            user.Senha = password;
            await _dataBaseContext.SaveChangesAsync();
            return user.Id;
          
        }

    }
}