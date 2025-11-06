namespace SqlToPdfWithAI.Dtos
{
    public class QueryResponseDto
    {
        public Guid ReportId { get; set; }
        public int RowCount { get; set; }
        public long DurationMs { get; set; }
        public string[] Columns { get; set; } = Array.Empty<string>();
        public object[]? Preview { get; set; }
    }
}
