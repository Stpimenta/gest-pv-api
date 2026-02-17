namespace IbpvDtos;

public class MonthlyFinancialSummaryDTO
{
    public int WalletId { get; set; }
    public string WalletName { get; set; } = string.Empty;

    public decimal BalancePreviousMonth { get; set; }
    public decimal TotalMonthlyContributions { get; set; }
    public decimal TotalMonthlyExpenses { get; set; }
    public decimal BalanceAtMonth { get; set; }
    public decimal BalanceAtNextMonth { get; set; }
}