using c___Api_Example.Models;

namespace IbpvDtos;

public class MonthlyFinancialReportDTO
{
    public int WalletId { get; set; }
    public string WalletName { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    
    public decimal BalancePreviousMonth { get; set; }
    public decimal TotalMonthlyContributions { get; set; }
    public decimal TotalMonthlyExpenses { get; set; }
    public decimal BalanceAtMonth { get; set; }
    public decimal BalanceAtNextMonth { get; set; }
    
    public List<ContribuicaoGetByIdDTO> Contributions { get; set; } = new();
    public List<GastoDTO> Expenses { get; set; } = new();
}