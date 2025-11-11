namespace SqlToPdfWithAI.Dtos
{
    public class QueryPersistModelDto
    {
        public Guid ReportId { get; set; }
        public string Sql { get; set; } = string.Empty;
        public string[] Columns { get; set; } = Array.Empty<string>();
        public List<Dictionary<string, object?>> Rows { get; set; } = new();
        public int RowCount { get; set; }
        public long DurationMs { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
