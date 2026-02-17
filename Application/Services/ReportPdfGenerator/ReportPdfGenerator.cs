using System.Globalization;
using System.Reflection.Metadata;
using IbpvDtos;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;


namespace c___Api_Example.Application.Services.generateReportPdf;

public class ReportPdfGenerator
{
    public MemoryStream GenerateFinancialReport(MonthlyFinancialReportDTO reportDto)
    {
        var basePath = AppContext.BaseDirectory;
        var logoPath = Path.Combine(basePath, "Resources", "Images", "IBPV.jpg");
        var selectBox = Path.Combine(basePath, "Resources", "Images", "selectBox.png");
        var ptBr = new CultureInfo("pt-BR");

        QuestPDF.Settings.License = LicenseType.Community;

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Size(PageSizes.A4);

                // header
                page.Header()
                    .BorderBottom(0.5f)
                    .AlignCenter()
                    .Height(100)
                    .Row(row =>
                    {
                        row.RelativeColumn().Width(180).Image(logoPath).FitArea();
                        row.RelativeColumn()
                            .AlignCenter()
                            .Column(column =>
                            {
                                column.Item().Text("Relatório Financeiro").FontSize(25);
                                column.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(reportDto.WalletName); // wallet name
                                    r.RelativeItem().Text(new DateTime(reportDto.Year, reportDto.Month, 1)
                                        .ToString("MMMM 'de' yyyy", new CultureInfo("pt-BR"))); // data
                                });
                            });
                    });

                // body
                var firstDayOfMonth = new DateTime(reportDto.Year, reportDto.Month, 1);
                var firstDayNextMonth = firstDayOfMonth.AddMonths(1);

                page.Content()
                    .AlignCenter()
                    .PaddingTop(15)
                    .Column(column =>
                    {
                      
                        column.Item().Row(r => r.RelativeColumn().Text("Entradas").AlignLeft().FontSize(20));
                        column.Item().AlignCenter().Row(r =>
                        {
                            r.ConstantColumn(15).Text("").Bold();
                            var hasToken = reportDto.Contributions.Any(c => !string.IsNullOrEmpty(c.TokenMembro));
                            r.ConstantColumn(250).Text(hasToken ? "Código de Dizimista" : "Descrição").Bold();
                            r.ConstantColumn(100).Text("Valor").Bold();
                        });

                        foreach (var c in reportDto.Contributions)
                        {
                            column.Item().AlignCenter().PaddingBottom(5).Row(r =>
                            {
                                r.ConstantColumn(20).Width(15).Height(15).Image(selectBox).FitArea();
                                r.ConstantColumn(250)
                                    .Text(string.IsNullOrEmpty(c.TokenMembro) ? c.Descricao : c.TokenMembro);
                                r.ConstantColumn(100).Text(c.Valor.ToString("C", ptBr));
                            });
                        }

                        column.Item().AlignCenter()
                            .Text($"Total de entradas: {reportDto.TotalMonthlyContributions.ToString("C", ptBr)}");

                        column.Item().AlignCenter()
                            .Text(
                                $"Saldo para {firstDayOfMonth.ToString("MMMM 'de' yyyy", ptBr)}: {reportDto.BalancePreviousMonth.ToString("C", ptBr)}");

                        column.Item().AlignCenter()
                            .Text($"Saldo Total: {reportDto.BalanceAtMonth.ToString("C", ptBr)}");

                        column.Item().PageBreak();

                    
                        column.Item().Row(r => r.RelativeColumn().Text("Saídas").AlignLeft().FontSize(20));
                        column.Item().AlignCenter().Row(r =>
                        {
                            r.ConstantColumn(250).Text("Descrição").Bold();
                            r.ConstantColumn(100).Text("Valor").Bold();
                        });

                        foreach (var g in reportDto.Expenses)
                        {
                            column.Item().AlignCenter().PaddingBottom(5).Row(r =>
                            {
                                r.ConstantColumn(20).Width(15).Height(15).Image(selectBox).FitArea();
                                r.ConstantColumn(250).Text(g.Descricao).FontSize(12).WrapAnywhere();
                                r.ConstantColumn(100).Text(g.Valor.ToString("C", ptBr));
                            });
                        }

                        column.Item().AlignCenter()
                            .Text($"Total de saídas: {reportDto.TotalMonthlyExpenses.ToString("C", ptBr)}");

                        column.Item().AlignCenter()
                            .Text(
                                $"Saldo para {firstDayNextMonth.ToString("MMMM 'de' yyyy", ptBr)}: {reportDto.BalanceAtNextMonth.ToString("C", ptBr)}");
                    });

                page.Footer().AlignRight().Text(txt =>
                {
                    txt.Span("Página ");
                    txt.CurrentPageNumber();
                    txt.Span(" de ");
                    txt.TotalPages();
                });
            });
        });

        var memoryStream = new MemoryStream();
        document.GeneratePdf(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public MemoryStream GeneratePdfBalanceSummary(
        int year,
        int month,
        List<MonthlyFinancialSummaryDTO> summaries)
    {
        var basePath = AppContext.BaseDirectory;
        var logoPath = Path.Combine(basePath, "Resources", "Images", "IBPV.jpg");
        QuestPDF.Settings.License = LicenseType.Community;

        var totalBalance = summaries.Sum(s => s.BalanceAtNextMonth);
        var ptBrCulture = new CultureInfo("pt-BR");

    
        var reportMonth = new DateTime(year, month, 1);

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Size(PageSizes.A4);

                // Header
                page.Header()
                    .BorderBottom(0.5f)
                    .AlignCenter()
                    .Height(100)
                    .Row(row =>
                    {
                        row.RelativeColumn().Width(180).Image(logoPath).FitArea();
                        row.RelativeColumn()
                            .AlignCenter()
                            .Column(column =>
                            {
                                column.Item().Text("Relatório Financeiro").FontSize(25);
                                column.Item().Text(reportMonth.ToString("MMMM 'de' yyyy", ptBrCulture));
                            });
                    });

                // Content
                page.Content()
                    .AlignCenter()
                    .PaddingTop(15)
                    .Column(column =>
                    {
                        column.Item().Text("Resumo de Saldos").FontSize(20);

                        foreach (var summary in summaries)
                        {
                            column.Item().PaddingBottom(5).Row(r =>
                            {
                                r.ConstantColumn(250).Text(summary.WalletName);
                                r.ConstantColumn(100).Text($"R$ {summary.BalanceAtNextMonth:N2}");
                            });
                        }

                        // Total
                        column.Item().PaddingBottom(15).Row(r =>
                        {
                            r.ConstantColumn(250).Text("Total Balance");
                            r.ConstantColumn(100).Text(totalBalance.ToString("C", ptBrCulture));
                        });

                        // Signatures
                        for (int i = 1; i <= 3; i++)
                        {
                            column.Item()
                                .AlignCenter()
                                .PaddingVertical(20)
                                .Width(250)
                                .BorderBottom(0.5f)
                                .Text($"Ass {i}:")
                                .FontSize(10);
                        }
                    });

                // Footer
                page.Footer().AlignRight().Text(txt =>
                {
                    txt.Span("Página ");
                    txt.CurrentPageNumber();
                    txt.Span(" de ");
                    txt.TotalPages();
                });
            });
        });

        var memoryStream = new MemoryStream();
        document.GeneratePdf(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }
}