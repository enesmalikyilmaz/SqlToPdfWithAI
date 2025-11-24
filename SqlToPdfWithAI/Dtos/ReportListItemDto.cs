namespace SqlToPdfWithAI.Dtos
{
    public class ReportListItemDto
    {
        public Guid ReportId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RowCount { get; set; }
        public long DurationMs { get; set; }
        public string SqlPreview { get; set; } = string.Empty;
    }
}
