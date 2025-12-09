using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SqlToPdfWithAI.Dtos;

namespace SqlToPdfWithAI.Services;

public static class PdfHelper
{
    // Uzun, boşluksuz metinleri kırabilmek için gizli boşluk ekler. 
    private static string SoftWrap(string input, int maxChunk = 30)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var s = input;
        // 30+ karakterlik boşluksuz parçaları \u200B ile parçalıyoruz
        for (int i = maxChunk; i < s.Length; i += (maxChunk + 1))
            s = s.Insert(i, "\u200B");
        return s;
    }

    private static string Truncate(string s, int max)
        => string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";

    public static string BuildReportPdf(QueryPersistModelDto data)
    {
        var storageRoot = Path.Combine(AppContext.BaseDirectory, "storage");
        Directory.CreateDirectory(storageRoot);
        var pdfPath = Path.Combine(storageRoot, $"{data.ReportId}.pdf");

        // QuestPDF genel ayarları
        QuestPDF.Settings.License = LicenseType.Community;

        
        GenerateFullPdf(data, pdfPath);
         return pdfPath;
    }

    private static void GenerateFullPdf(QueryPersistModelDto data, string pdfPath)
    {
        // Tabloyu güvene almak için kolon/satır limitleri
        var cols = (data.Columns ?? Array.Empty<string>()).Take(8).ToArray(); // <= 8 kolon
        var rows = data.Rows ?? new List<Dictionary<string, object?>>();
        var rowCount = Math.Min(50, rows.Count); // <= 50 satır

        Document.Create(container =>
        {
            // --- Sayfa 1: Kapak/Özet ---
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.Header().Text(data.ReportName).SemiBold().FontSize(22);

                page.Content().Column(col =>
                {
                    col.Spacing(6);
                    col.Item().Text($"Rapor ID: {data.ReportId}");
                    col.Item().Text($"Tarih: {DateTime.Now:yyyy-MM-dd HH:mm}");
                    col.Item().Text($"Satır: {data.RowCount}");
                    col.Item().Text($"Süre: {data.DurationMs} ms");
                    col.Item().Text("Sorgu:").SemiBold();
                    col.Item().Text(SoftWrap(data.Sql ?? string.Empty)).FontSize(10).FontColor(Colors.Grey.Darken2);
                });

                page.Footer().AlignRight().Text(t =>
                {
                    t.Span("SqlToPdfWithAI • ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });

            // --- Sayfa 2: Grafik ---
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.Header().Text("Grafik").SemiBold().FontSize(16);

                var chartPath = Path.Combine(AppContext.BaseDirectory, "storage", $"{data.ReportId}_chart1.png");
                if (File.Exists(chartPath))
                    page.Content().Image(chartPath, ImageScaling.FitArea);
                else
                    page.Content().Text("Uygun veri bulunamadığı için grafik üretilmedi.")
                                  .FontColor(Colors.Grey.Darken2);

                page.Footer().AlignRight().Text(t =>
                {
                    t.Span("SqlToPdfWithAI • ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });

            // --- Sayfa 3: Tablo (ilk 50 satır, ilk 8 kolon) ---
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(15); // tabloya biraz daha alan verdik
                page.Header().Text("Sonuçlar (ilk 50 satır)").SemiBold().FontSize(16);

                page.Content().Column(c =>
                {
                    c.Spacing(5);

                    // Kolon yoksa kullanıcıya mesaj
                    if (cols.Length == 0 || rowCount == 0)
                    {
                        c.Item().Text("Gösterilecek veri bulunamadı.").FontColor(Colors.Grey.Darken2);
                        return;
                    }

                    // Table'i doğrudan Content'e koyuyoruz
                    c.Item().Table(table =>
                    {
                        // Kolon tanımı
                        table.ColumnsDefinition(cd =>
                        {
                            foreach (var _ in cols)
                                cd.RelativeColumn();
                        });

                        // Header
                        table.Header(header =>
                        {
                            foreach (var name in cols)
                                header.Cell().Element(HeaderCell).Text(SoftWrap(name));

                            static IContainer HeaderCell(IContainer container) =>
                                container.DefaultTextStyle(x => x.SemiBold())
                                         .Padding(4)
                                         .Background(Colors.Grey.Lighten3)
                                         .Border(1)
                                         .BorderColor(Colors.Grey.Lighten1);
                        });

                        // Satırlar
                        for (int i = 0; i < rowCount; i++)
                        {
                            var row = rows[i];
                            foreach (var name in cols)
                            {
                                var raw = row.TryGetValue(name, out var v) ? v?.ToString() ?? "" : "";
                                var txt = SoftWrap(Truncate(raw, 80)); // uzunları kır + kısalt
                                table.Cell().Element(BodyCell).Text(txt);

                                static IContainer BodyCell(IContainer container) =>
                                    container.Padding(3)
                                             .BorderBottom(1)
                                             .BorderColor(Colors.Grey.Lighten3);
                            }
                        }
                    });
                });

                page.Footer().AlignRight().Text(t =>
                {
                    t.Span("SqlToPdfWithAI • ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });

        }).GeneratePdf(pdfPath);
    }

    
}