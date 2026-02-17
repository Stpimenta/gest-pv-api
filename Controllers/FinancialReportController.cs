using c___Api_Example.Application.Services.ControllersServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace c___Api_Example.Controllers;

[ApiController]
[Route("api/financial-report")]
[Authorize]
public class FinancialReportController : ControllerBase
{
    private readonly FinancialReportService _financialReportService;

    public FinancialReportController(FinancialReportService financialReportService)
    {
        _financialReportService = financialReportService;
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int walletId, [FromQuery] int year,
        [FromQuery] int month)
    {
        var report = await _financialReportService.GetMonthlyFinancialReport(walletId, year, month);
        return Ok(report);
    }

    [HttpGet("monthly/pdf")]
    public async Task<IActionResult> DownloadMonthlyReportPdf([FromQuery] int walletId, [FromQuery] int year, [FromQuery] int month)
    {
        var pdfStream = await _financialReportService.GetMonthlyFinancialReportPdf(walletId, year, month);
        return File(pdfStream, "application/pdf", $"Relatorio_{walletId}_{month}_{year}.pdf");
    }

    [HttpGet("monthly/summary/pdf")]
    public async Task<IActionResult> DownloadMonthlySummaryPdf([FromQuery] int year, [FromQuery] int month)
    {
        var pdfStream = await _financialReportService.GetMonthlyBalanceSummaryPdf(year, month);
        return File(pdfStream, "application/pdf", $"Resumo_Saldos_{month}_{year}.pdf");
    }
}