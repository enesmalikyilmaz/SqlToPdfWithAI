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
        public IActionResult Get(Guid id)
        {
            var jsonPath = Path.Combine("storage", $"{id}.json");
            if (!System.IO.File.Exists(jsonPath))
                return NotFound("Rapor verisi bulunamadı.");

            // JSON'u oku
            var json = System.IO.File.ReadAllText(jsonPath);
            var data = System.Text.Json.JsonSerializer.Deserialize<QueryPersistModelDto>(json);
            if (data is null)
                return StatusCode(500, "Rapor verisi okunamadı.");

            // PDF'i oluştur (her çağrıda yeniden üretir)
            var pdfPath = PdfHelper.BuildReportPdf(data);
            var fileBytes = System.IO.File.ReadAllBytes(pdfPath);

            return File(fileBytes, "application/pdf", $"{id}.pdf");
        }
    }
}
