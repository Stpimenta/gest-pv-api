using IbpvDtos;

namespace c___Api_Example.Infrastructure.Repository.Interfaces;

public interface IFinancialReportRepository
{
    public Task<MonthlyFinancialReportDTO> GetMonthlyFinancialReport(int walletId, int year, int month);

    public Task<List<MonthlyFinancialSummaryDTO>> GetAllWalletsMonthlySummary(int year, int month);
}