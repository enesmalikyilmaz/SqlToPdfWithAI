using ScottPlot;
using System.Globalization;

namespace SqlToPdfWithAI.Services;

public static class ChartHelper
{
    
    /// data: satır listesi (kolon adı -> değer)
    /// columns: kolon isimleri (sıralı)
    /// storageRoot: env.ContentRootPath/storage
    
    public static List<string> RenderCharts(
        List<Dictionary<string, object?>> data,
        string[] columns,
        Guid reportId,
        string storageRoot)
    {
        var paths = new List<string>();
        if (data.Count == 0 || columns.Length == 0) return paths;

        // 1) datetime + numeric => Line
        var (dtCol, numCol) = FindDateTimeAndNumericColumns(data, columns);
        if (dtCol is not null && numCol is not null)
        {
            var path = Path.Combine(storageRoot, $"{reportId}_chart1.png");
            TryLineChartV4(data, dtCol!, numCol!, path);
            if (File.Exists(path)) paths.Add(path);
        }
        else
        {
            // 2) string + numeric => Bar (ilk 10 kategori)
            var (catCol, valCol) = FindCategoryAndNumericColumns(data, columns);
            if (catCol is not null && valCol is not null)
            {
                var path = Path.Combine(storageRoot, $"{reportId}_chart1.png");
                TryBarChartTopKV4(data, catCol!, valCol!, 10, path);
                if (File.Exists(path)) paths.Add(path);
            }
        }

        // en fazla 1 grafik: okunabilir kalsın.
        return paths;
    }

    private static (string? dtCol, string? numCol) FindDateTimeAndNumericColumns(
        List<Dictionary<string, object?>> data, string[] columns)
    {
        foreach (var c1 in columns)
        {
            // ilk 20 satırdan tip tahmini
            bool looksDate = data.Take(20).Any(r => DataTypeHelper.IsDate(r.TryGetValue(c1, out var v) ? v : null));
            if (!looksDate) continue;

            foreach (var c2 in columns)
            {
                if (c2 == c1) continue;
                bool looksNum = data.Take(20).Any(r => DataTypeHelper.IsNumeric(r.TryGetValue(c2, out var v2) ? v2 : null));
                if (looksNum) return (c1, c2);
            }
        }
        return (null, null);
    }

    private static (string? catCol, string? numCol) FindCategoryAndNumericColumns(
        List<Dictionary<string, object?>> data, string[] columns)
    {
        foreach (var c1 in columns)
        {
            // kategori: string gibi görünen
            bool looksCat = data.Take(20).Any(r => (r.TryGetValue(c1, out var v) ? v : null) is string);
            if (!looksCat) continue;

            foreach (var c2 in columns)
            {
                if (c2 == c1) continue;
                bool looksNum = data.Take(20).Any(r => DataTypeHelper.IsNumeric(r.TryGetValue(c2, out var v2) ? v2 : null));
                if (looksNum) return (c1, c2);
            }
        }
        return (null, null);
    }

    private static void TryLineChartV4(List<Dictionary<string, object?>> data, string dtCol, string numCol, string path)
    {
        try
        {
            var pts = new List<(DateTime t, double y)>();
            foreach (var row in data)
            {
                if (!row.TryGetValue(dtCol, out var tv) || !row.TryGetValue(numCol, out var yv)) continue;

                DateTime t;
                if (tv is DateTime dt) t = dt;
                else if (tv is string ts && DateTime.TryParse(ts, out var dt2)) t = dt2;
                else continue;

                if (!TryToDouble(yv, out var y)) continue;

                pts.Add((t, y));
            }
            if (pts.Count < 2) return;

            pts = pts.OrderBy(p => p.t).ToList();
            double[] xs = pts.Select(p => p.t.ToOADate()).ToArray();
            double[] ys = pts.Select(p => p.y).ToArray();

            var plt = new ScottPlot.Plot(900, 500);
            plt.AddScatter(xs, ys);
            plt.XAxis.DateTimeFormat(true);      
            plt.Title($"{dtCol} - {numCol}");
            plt.XLabel(dtCol);
            plt.YLabel(numCol);

            plt.SaveFig(path, 900, 500);         
        }
        catch { }
    }

    private static void TryBarChartTopKV4(List<Dictionary<string, object?>> data, string catCol, string numCol, int k, string path)
    {
        try
        {
            var groups = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in data)
            {
                if (!row.TryGetValue(catCol, out var cv) || !(cv is string key)) continue;
                if (!row.TryGetValue(numCol, out var v)) continue;
                if (!TryToDouble(v, out var d)) continue;

                groups[key] = groups.TryGetValue(key, out var cur) ? cur + d : d;
            }
            if (groups.Count == 0) return;

            var top = groups.OrderByDescending(kv => kv.Value).Take(k).ToList();
            double[] values = top.Select(kv => kv.Value).ToArray();
            string[] labels = top.Select(kv => kv.Key).ToArray();

            var plt = new ScottPlot.Plot(900, 500);
            var bar = plt.AddBar(values);
            

            plt.XTicks(labels);                  
            plt.XAxis.TickLabelStyle(rotation: 45);
            plt.Title($"{catCol} - {numCol} (Top {k})");
            plt.YLabel(numCol);

            plt.SaveFig(path, 900, 500);         
        }
        catch {}
    }


    private static bool TryToDouble(object? v, out double d)
    {
        if (v is null) { d = 0; return false; }
        switch (v)
        {
            case byte b: d = b; return true;
            case sbyte sb: d = sb; return true;
            case short s: d = s; return true;
            case ushort us: d = us; return true;
            case int i: d = i; return true;
            case uint ui: d = ui; return true;
            case long l: d = l; return true;
            case ulong ul: d = ul; return true;
            case float f: d = f; return true;
            case double dd: d = dd; return true;
            case decimal dc: d = (double)dc; return true;
            case string str when double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var x):
                d = x; return true;
            default: d = 0; return false;
        }
    }
}
