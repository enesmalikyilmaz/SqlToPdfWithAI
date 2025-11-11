using Microsoft.AspNetCore.Mvc;
using SqlToPdfWithAI.Dtos;
using SqlToPdfWithAI.Services;

namespace SqlToPdfWithAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly DbQueryService _db;
    private readonly ILogger<QueryController> _log;
    private readonly AppLimits _limits;

    public QueryController(
        DbQueryService db,
        ILogger<QueryController> log,
        Microsoft.Extensions.Options.IOptions<AppLimits> limits)
    {
        _db = db;
        _log = log;
        _limits = limits.Value;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] QueryRequestDto req)
    {
        if (req is null || string.IsNullOrWhiteSpace(req.Sql))
            return BadRequest("SQL boş olamaz.");

        // Basit kontrol: SELECT ile başlamalı ve bazı tehlikeli kelimeler yoksa geç
        if (!QueryGuard.IsSafeSelect(req.Sql))
            return BadRequest("Sadece SELECT sorgularına izin verilir.");

        try
        {
            var (rows, columns, ms) = await _db.RunSelectAsync(req.Sql);

            var res = new QueryResponseDto
            {
                ReportId = Guid.NewGuid(),
                RowCount = rows.Count,
                DurationMs = ms,
                Columns = columns,
                Preview = rows.Take(20).Cast<object>().ToArray()
            };

            var persist = new QueryPersistModelDto
            {
                ReportId = res.ReportId,
                Sql = req.Sql,
                Columns = res.Columns,
                Rows = rows.Select(r => ((IDictionary<string, object>)r)
                                  .ToDictionary(kv => kv.Key, kv => kv.Value)).ToList(),
                RowCount = res.RowCount,
                DurationMs = res.DurationMs,
                CreatedAt = DateTime.Now
            };

            Directory.CreateDirectory("storage");
            var jsonPath = Path.Combine("storage", $"{res.ReportId}.json");
            await System.IO.File.WriteAllTextAsync(
                jsonPath,
                System.Text.Json.JsonSerializer.Serialize(
                    persist,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = false }
                )
            );

            _log.LogInformation("Query ok. rows={RowCount} ms={Ms}", res.RowCount, res.DurationMs);
            return Ok(res);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Query failed");
            return StatusCode(500, "Sorgu çalıştırılırken hata oluştu.");
        }
    }
}
