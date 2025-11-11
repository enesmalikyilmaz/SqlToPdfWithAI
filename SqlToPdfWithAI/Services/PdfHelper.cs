using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SqlToPdfWithAI.Dtos;

namespace SqlToPdfWithAI.Services;

public static class PdfHelper
{
    public static string BuildReportPdf(QueryPersistModelDto data)
    {
        Directory.CreateDirectory("storage");
        var pdfPath = Path.Combine("storage", $"{data.ReportId}.pdf");

        // QuestPDF lisans hatırlatması gerekirse (Free Community kullanıyoruz)
        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(container =>
        {
            // Sayfa 1: Kapak / Özet
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text("SQL Raporu")
                    .SemiBold().FontSize(22);

                page.Content().Column(col =>
                {
                    col.Item().Text($"Rapor ID: {data.ReportId}");
                    col.Item().Text($"Tarih: {DateTime.Now:yyyy-MM-dd HH:mm}");
                    col.Item().Text($"Satır: {data.RowCount}");
                    col.Item().Text($"Süre: {data.DurationMs} ms");
                    col.Item().Text(" ");
                    col.Item().Text("Sorgu:")
                        .SemiBold();
                    col.Item().Text(data.Sql)
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken2);
                });

                page.Footer().AlignRight().Text(t =>
                {
                    t.Span("SqlToPdfWithAI • ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });

            // Sayfa 2: (İlk 50 satır) Tablo
            container.Page(page =>
            {
                page.Margin(20);

                page.Header().Text("Sonuçlar (ilk 50 satır)")
                    .SemiBold().FontSize(16);

                page.Content().Table(table =>
                {
                    // Kolonlar
                    table.ColumnsDefinition(cd =>
                    {
                        foreach (var _ in data.Columns)
                            cd.RelativeColumn();
                    });

                    // Başlık
                    table.Header(header =>
                    {
                        foreach (var c in data.Columns)
                            header.Cell().Element(CellHeader).Text(c);

                        static IContainer CellHeader(IContainer container)
                        {
                            return container
                                .DefaultTextStyle(x => x.SemiBold())
                                .Padding(5)
                                .Background(Colors.Grey.Lighten3)
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten1);
                        }
                    });

                    // Satırlar (max 50)
                    var take = Math.Min(50, data.Rows.Count);
                    for (int i = 0; i < take; i++)
                    {
                        var row = data.Rows[i];
                        foreach (var c in data.Columns)
                        {
                            var val = row.TryGetValue(c, out var v) ? v : null;
                            table.Cell().Element(CellBody).Text(val?.ToString() ?? "");

                            static IContainer CellBody(IContainer container)
                            {
                                return container
                                    .Padding(4)
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Grey.Lighten3);
                            }
                        }
                    }
                });

                page.Footer().AlignRight().Text(t =>
                {
                    t.Span("SqlToPdfWithAI • ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        })
        .GeneratePdf(pdfPath);

        return pdfPath;
    }
}
