using c___Api_Example.Application.Services.generateReportPdf;
using c___Api_Example.Infrastructure.Repository;
using c___Api_Example.Infrastructure.Repository.Interfaces;
using c___Api_Example.Repository.Interfaces;
using IbpvDtos;

namespace c___Api_Example.Application.Services.ControllersServices;

public class FinancialReportService
{
    private readonly IFinancialReportRepository _repository;
    private readonly ICaixaRepositorio _walletRepository;
    private readonly ReportPdfGenerator _pdfGenerator;

    public FinancialReportService( IFinancialReportRepository repository, ICaixaRepositorio walletRepository,  ReportPdfGenerator pdfGenerator)
    {
        _repository = repository;
        _walletRepository = walletRepository;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<MonthlyFinancialReportDTO> GetMonthlyFinancialReport(int walletId, int year, int month)
    {
        // wallet id
        if (walletId <= 0)
            throw new ArgumentException("Invalid walletId");

        // year
        if (year < 2000 || year > DateTime.UtcNow.Year + 1)
            throw new ArgumentException("Invalid year");

        // month
        if (month < 1 || month > 12)
            throw new ArgumentException("Invalid month");

        // wallet existence
        var wallets = await _walletRepository.GetAllCaixas();
        var walletExists = wallets.Any(c => c.Id == walletId);
        
        if (!walletExists)
            throw new Exception("Wallet not found");

        return await _repository.GetMonthlyFinancialReport(
            walletId,
            year,
            month);
    }
    
    public async Task<(MemoryStream Stream, string FileName)> GetMonthlyFinancialReportPdf(int walletId, int year, int month)
    {
        var reportDto = await GetMonthlyFinancialReport(walletId, year, month);
        var stream = _pdfGenerator.GenerateFinancialReport(reportDto);
        return (stream, BuildReportFileName(reportDto.WalletName, year, month));
    }
    
    public async Task<(MemoryStream Stream, string FileName)> GetMonthlyBalanceSummaryPdf(int year, int month)
    {
      
        var summaries = await _repository.GetAllWalletsMonthlySummary(year, month);
        var stream = _pdfGenerator.GeneratePdfBalanceSummary(year, month, summaries);
        return (stream, BuildReportFileName("Resumo_Saldos", year, month));
    }

    private static readonly string[] MonthAbbreviations =
    {
        "Jan", "Fev", "Mar", "Abr", "Mai", "Jun",
        "Jul", "Ago", "Set", "Out", "Nov", "Dez"
    };

    private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars()
        .Concat(new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' })
        .Distinct()
        .ToArray();

    private static string BuildReportFileName(string baseName, int year, int month)
    {
        var safeBaseName = string.Concat(baseName.Where(c => !InvalidFileNameChars.Contains(c))).Trim();
        if (string.IsNullOrWhiteSpace(safeBaseName))
            safeBaseName = "Relatorio";

        var monthLabel = MonthAbbreviations[Math.Clamp(month, 1, 12) - 1];
        return $"{safeBaseName}_{monthLabel} {year}.pdf";
    }
}