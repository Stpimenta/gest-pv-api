using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using c___Api_Example.Models;
using IbpvDtos;

namespace c___Api_Example.Repository.Interfaces
{
    public interface ICaixaRepositorio
    {
        Task<List<CaixaModel>> GetAllCaixas();
        Task<int> AddCaixa(CaixaModel caixa);
        Task<bool> DeleteCaixa(int id);
        
        Task<int> UpdateCaixa(int id, CaixaPutDto  caixaDTO);
    }
}