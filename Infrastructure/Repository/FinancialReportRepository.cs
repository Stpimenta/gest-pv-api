using c___Api_Example.data;
using c___Api_Example.Infrastructure.Repository.Interfaces;
using IbpvDtos;
using Microsoft.EntityFrameworkCore;

namespace c___Api_Example.Infrastructure.Repository;

public class FinancialReportRepository:IFinancialReportRepository
{
    private readonly IbpvDataBaseContext _repo;

    public FinancialReportRepository( IbpvDataBaseContext repo)
    {
        _repo = repo;
    }
    public async Task<MonthlyFinancialReportDTO> GetMonthlyFinancialReport(int walletId, int year, int month)
    {
        var wallet = await  _repo.Caixa.FindAsync(walletId);
        
        if(wallet is null)
            throw new Exception("Wallet not find");
        
        var cutoffDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(-1); //last day of the previous month

        var allExpensesAfterThisMonth = await _repo.Gasto
            .Where(g => g.IdCaixa == wallet.Id && g.Data > cutoffDate)
            .SumAsync(g => g.Valor);
        
        var allContributionsAfterThisMonth = await _repo.Contribuicao
            .Where(c => c.IdCaixa == wallet.Id && c.Data > cutoffDate)
            .SumAsync(g => g.Valor);

        decimal balancePreviousMonth = wallet.ValorTotal + allExpensesAfterThisMonth - allContributionsAfterThisMonth;
        
        
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate   = startDate.AddMonths(1);
        
        
        var monthlyExpenses = await _repo.Gasto
            .Include(c => c.Images)
            .Where(g =>
                g.IdCaixa == wallet.Id &&
                g.Data >= startDate &&
                g.Data < endDate
            )
            .OrderBy(c => c.Data)
            .ToListAsync();

        var totalMonthlyExpenses = monthlyExpenses.Sum(g => g.Valor);
        
        
        var monthlyContributions = await _repo.Contribuicao
            .Include(c => c.Images)
            .Include(c => c.Membro)
            .Where(c =>
                c.IdCaixa == wallet.Id &&
                c.Data >= startDate &&
                c.Data < endDate
            )
            .OrderBy(c => c.Data)
            .ToListAsync();

        var totalMonthlyContributions = monthlyContributions.Sum(c => c.Valor);

        decimal balanceAtMonth = balancePreviousMonth + totalMonthlyContributions;
        decimal balanceAtNextMonth = balancePreviousMonth + totalMonthlyContributions - totalMonthlyExpenses;
        
        
        //map
        var contributionDtos = monthlyContributions
            .Select(c => new ContribuicaoGetByIdDTO
            {
                Id = c.Id,
                Valor = c.Valor,
                Descricao = c.Descricao,
                Data = c.Data,
                IdCaixa = c.IdCaixa,
                IdMembro = c.IdMembro,
                TokenMembro = c.Membro?.TokenContribuicao,

                // Images = c.Images
                //     .Select(img => new ContribuicaoImageDTO
                //     {
                //         Id = img.Id,
                //         Url = img.Url
                //     })
                //     .ToList()
            })
            .ToList();
        
        
        var expenseDtos = monthlyExpenses
            .Select(g => new GastoDTO
            {
                Id = g.Id,
                Valor = g.Valor,
                Descricao = g.Descricao,
                Data = g.Data,

                // Images = g.Images
                //     .Select(img => new ExpenseImageDTO
                //     {
                //         Id = img.Id,
                //         Url = img.Url
                //     })
                //     .ToList()
            })
            .ToList();
        
        return new MonthlyFinancialReportDTO
        {
            WalletId = wallet.Id,
            WalletName = wallet.Nome!,
            Year = year,
            Month = month,
            
            BalancePreviousMonth =  balancePreviousMonth,
            TotalMonthlyContributions =  totalMonthlyContributions,
            TotalMonthlyExpenses =  totalMonthlyExpenses,
            BalanceAtMonth =  balanceAtMonth,
            BalanceAtNextMonth =  balanceAtNextMonth,
            
            Contributions =  contributionDtos,
            Expenses = expenseDtos
        };
    }
    
    
    public async Task<List<MonthlyFinancialSummaryDTO>> GetAllWalletsMonthlySummary(int year, int month)
    {
        var wallets = await _repo.Caixa.ToListAsync();

        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1);

        var summaries = new List<MonthlyFinancialSummaryDTO>();

        foreach (var wallet in wallets)
        {
            // calcular saldo do mês anterior
            var cutoffDate = startDate.AddTicks(-1);

            var allExpensesAfterThisMonth = await _repo.Gasto
                .Where(g => g.IdCaixa == wallet.Id && g.Data > cutoffDate)
                .SumAsync(g => g.Valor);

            var allContributionsAfterThisMonth = await _repo.Contribuicao
                .Where(c => c.IdCaixa == wallet.Id && c.Data > cutoffDate)
                .SumAsync(g => g.Valor);

            decimal balancePreviousMonth = wallet.ValorTotal + allExpensesAfterThisMonth - allContributionsAfterThisMonth;

            // total do mês
            var totalMonthlyExpenses = await _repo.Gasto
                .Where(g => g.IdCaixa == wallet.Id && g.Data >= startDate && g.Data < endDate)
                .SumAsync(g => g.Valor);

            var totalMonthlyContributions = await _repo.Contribuicao
                .Where(c => c.IdCaixa == wallet.Id && c.Data >= startDate && c.Data < endDate)
                .SumAsync(c => c.Valor);

            decimal balanceAtMonth = balancePreviousMonth + totalMonthlyContributions;
            decimal balanceAtNextMonth = balanceAtMonth - totalMonthlyExpenses;

            summaries.Add(new MonthlyFinancialSummaryDTO
            {
                WalletId = wallet.Id,
                WalletName = wallet.Nome!,
                BalancePreviousMonth = balancePreviousMonth,
                TotalMonthlyContributions = totalMonthlyContributions,
                TotalMonthlyExpenses = totalMonthlyExpenses,
                BalanceAtMonth = balanceAtMonth,
                BalanceAtNextMonth = balanceAtNextMonth
            });
        }

        return summaries;
    }
    
}