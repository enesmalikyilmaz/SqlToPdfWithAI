using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlToPdfWithAI.Dtos;
using SqlToPdfWithAI.Services;

namespace SqlToPdfWithAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public IActionResult Get(Guid id, [FromQuery] string? x = null, [FromQuery] string? y = null)
        {
            var storageRoot = Path.Combine(AppContext.BaseDirectory, "storage");
            Directory.CreateDirectory(storageRoot);

            var jsonPath = Path.Combine(storageRoot, $"{id}.json");
            if (!System.IO.File.Exists(jsonPath))
                return NotFound("Rapor verisi bulunamadı.");

            // JSON'u oku
            var json = System.IO.File.ReadAllText(jsonPath);
            var data = System.Text.Json.JsonSerializer.Deserialize<QueryPersistModelDto>(json);
            if (data is null)
                return StatusCode(500, "Rapor verisi okunamadı.");

            try
            {
                var rows = data.Rows ?? new List<Dictionary<string, object?>>();
                var cols = data.Columns ?? Array.Empty<string>();

                // Querystring boş ise null'a çevir
                var xCol = string.IsNullOrWhiteSpace(x) ? null : x;
                var yCol = string.IsNullOrWhiteSpace(y) ? null : y;

                ChartHelper.RenderCharts(
                    rows,
                    cols,
                    data.ReportId,
                    storageRoot,
                    xCol,
                    yCol
                );
            }
            catch
            {
            }

            // Her çağrıda yeniden üretilen PDF'i üret
            var pdfPath = PdfHelper.BuildReportPdf(data);
            var fileBytes = System.IO.File.ReadAllBytes(pdfPath);

            return File(fileBytes, "application/pdf", $"{id}.pdf");
        }
    }
}
