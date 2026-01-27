using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using c___Api_Example.Data.Map;
using c___Api_Example.Domain.Models;
using c___Api_Example.Infrastructure.Repository.Data.Map;
using c___Api_Example.Models;
using Microsoft.EntityFrameworkCore;

namespace c___Api_Example.data
{
    public class IbpvDataBaseContext : DbContext
    {
        public IbpvDataBaseContext (DbContextOptions<IbpvDataBaseContext> options)
            :base(options) //base é usado para passar para o contrutor da classe pai
        {

        }

        public DbSet<UsuarioModel> Usuarios{get; set;}
        public DbSet<ContribuicaoModel> Contribuicao{get; set;}
        public DbSet<CaixaModel> Caixa{get; set;}
        public DbSet<GastoModel> Gasto{get; set;}
        
        public DbSet<ExpenseImageModel> ExpenseImages { get; set; }
        public DbSet<ContributionImageModel> ContributionImages { get; set; }
        
        public DbSet<BlockedPeriodModel>  BlockedPeriod {get; set;}
        
        public DbSet<PendingUnlockModel>  PendingUnlock {get; set;}
        
        public DbSet<DesligamentoModel>  Desligamento {get; set;}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UsuarioMap());
            modelBuilder.ApplyConfiguration(new CaixaMap());
            modelBuilder.ApplyConfiguration(new GastoMap());
            modelBuilder.ApplyConfiguration(new ContribuicaoMap());
            modelBuilder.ApplyConfiguration(new BlockedPeriodsMap());
            modelBuilder.ApplyConfiguration(new PendingUnlockMap());
            modelBuilder.ApplyConfiguration(new DesligamentoMap());
            
            
            modelBuilder.ApplyConfiguration(new ExpenseImageMap());
            modelBuilder.ApplyConfiguration(new ContributionImageMap());
            
            base.OnModelCreating(modelBuilder);
        }



    }
}