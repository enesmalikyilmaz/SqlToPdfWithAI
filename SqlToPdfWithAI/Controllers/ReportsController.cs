using Microsoft.AspNetCore.Mvc;
using SqlToPdfWithAI.Dtos;
using SqlToPdfWithAI.Services;
using System.Text.Json;

namespace SqlToPdfWithAI.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index(string? q)
        {
            var storage = Path.Combine(AppContext.BaseDirectory, "storage");
            Directory.CreateDirectory(storage);

            var files = Directory.GetFiles(storage, "*.json");

            var list = new List<QueryPersistModelDto>();

            foreach (var file in files)
            {
                try
                {
                    var json = System.IO.File.ReadAllText(file);
                    var obj = JsonSerializer.Deserialize<QueryPersistModelDto>(json);

                    if (obj != null)
                        list.Add(obj);
                }
                catch { }
            }

            // Arama (q boş değilse)
            if (!string.IsNullOrWhiteSpace(q))
            {
                var lower = q.ToLower();
                list = list
                    .Where(x =>
                        x.ReportId.ToString().Contains(lower) ||
                        x.ReportName.ToLower().Contains(lower) ||
                        (x.Sql?.ToLower().Contains(lower) ?? false)
                    ).ToList();
            }

            // En yeni raporlar en yukarı gidecek şekilde.
            list = list.OrderByDescending(x => x.CreatedAt).ToList();

            return View(list);
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var storage = Path.Combine(AppContext.BaseDirectory, "storage");
            Directory.CreateDirectory(storage);

            var jsonPath = Path.Combine(storage, $"{id}.json");
            if (!System.IO.File.Exists(jsonPath))
                return NotFound("Rapor bulunamadı.");

            var json = System.IO.File.ReadAllText(jsonPath);
            var data = JsonSerializer.Deserialize<QueryPersistModelDto>(json);
            if (data == null)
                return StatusCode(500, "Rapor verisi okunamadı.");

            var vm = new ReportRenameViewModelDto
            {
                ReportId = data.ReportId,
                ReportName = data.ReportName
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ReportRenameViewModelDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var storage = Path.Combine(AppContext.BaseDirectory, "storage");
            Directory.CreateDirectory(storage);

            var jsonPath = Path.Combine(storage, $"{model.ReportId}.json");
            if (!System.IO.File.Exists(jsonPath))
                return NotFound("Rapor bulunamadı.");

            var json = System.IO.File.ReadAllText(jsonPath);
            var data = JsonSerializer.Deserialize<QueryPersistModelDto>(json);
            if (data == null)
                return StatusCode(500, "Rapor verisi okunamadı.");

            data.ReportName = string.IsNullOrWhiteSpace(model.ReportName)
                ? "Adsız Rapor"
                : model.ReportName.Trim();

            var updatedJson = JsonSerializer.Serialize(data);
            System.IO.File.WriteAllText(jsonPath, updatedJson);

            return RedirectToAction("Index");
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetPdf(Guid id)
        {
            var storage = Path.Combine(AppContext.BaseDirectory, "storage");
            Directory.CreateDirectory(storage);

            var jsonPath = Path.Combine(storage, $"{id}.json");
            if (!System.IO.File.Exists(jsonPath))
                return NotFound("Rapor verisi bulunamadı.");

            var json = System.IO.File.ReadAllText(jsonPath);
            var data = JsonSerializer.Deserialize<QueryPersistModelDto>(json);
            if (data is null)
                return StatusCode(500, "Rapor verisi okunamadı.");

            var pdfPath = PdfHelper.BuildReportPdf(data); 
            var bytes = System.IO.File.ReadAllBytes(pdfPath);
            return File(bytes, "application/pdf", $"{id}.pdf");
        }


    }
}
