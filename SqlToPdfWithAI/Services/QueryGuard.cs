using System.Text.RegularExpressions;

namespace SqlToPdfWithAI.Services;

public static class QueryGuard
{
    public static bool IsSafeSelect(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

        // SELECT ile başlasın
        if (!Regex.IsMatch(sql, @"^\s*SELECT\b", RegexOptions.IgnoreCase))
            return false;

        // En temel kara liste
        var forbidden = new[] { "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "TRUNCATE", "EXEC", "MERGE", "CREATE" };
        var upper = " " + sql.ToUpperInvariant() + " ";
        foreach (var f in forbidden)
        {
            if (upper.Contains(" " + f + " "))
                return false;
        }

        // Çoklu komutları kaba engelleme
        if (sql.Contains(";")) return false;

        return true;
    }

    public static string EnforceRowLimit(string sql, int maxRows, string dbKind = "SqlServer")
    {
        if (maxRows <= 0) return sql;

        // SQL Server varsay, TOP yoksa ekle
        if (!Regex.IsMatch(sql, @"\bTOP\s+\d+\b", RegexOptions.IgnoreCase))
            return Regex.Replace(sql, @"^\s*SELECT\s+(DISTINCT\s+)?",
                m => $"SELECT {m.Groups[1].Value}TOP {maxRows} ",
                RegexOptions.IgnoreCase);

        return sql;
    }
}
