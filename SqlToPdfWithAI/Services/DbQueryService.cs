using System.Data;
using System.Diagnostics;
using System.Dynamic;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace SqlToPdfWithAI.Services;

public class AppLimits
{
    public int MaxRows { get; set; } = 5000;
    public int CommandTimeoutSec { get; set; } = 15;
}

public class DbQueryService
{
    private readonly string _connString;
    private readonly AppLimits _limits;

    public DbQueryService(IConfiguration cfg, IOptions<AppLimits> limits)
    {
        _connString = cfg.GetConnectionString("ReadOnlyDb") ?? throw new Exception("ReadOnlyDb missing");
        _limits = limits.Value;
    }

    public async Task<(List<dynamic> Rows, string[] Columns, long Ms)> RunSelectAsync(string sql)
    {
        // Limiti uygula
        var limitedSql = QueryGuard.EnforceRowLimit(sql, _limits.MaxRows, "SqlServer");

        using var conn = new SqlConnection(_connString);
        

        var sw = Stopwatch.StartNew();
        var data = await conn.QueryAsync(limitedSql, commandTimeout: _limits.CommandTimeoutSec);
        sw.Stop();

        var list = data.ToList();

        // Kolon çıkarımı
        string[] columns = Array.Empty<string>();
        if (list.Count > 0)
        {
            var first = (IDictionary<string, object>)list[0];
            columns = first.Keys.ToArray();
        }

        return (list, columns, sw.ElapsedMilliseconds);
    }
}
