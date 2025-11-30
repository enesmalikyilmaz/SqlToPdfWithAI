namespace SqlToPdfWithAI.Dtos
{
    public class QueryRequestDto
    {
        public string Sql { get; set; } = string.Empty;
        public string ReportName { get; set; } = string.Empty;
        public string? XColumn { get; set; }
        public string? YColumn { get; set; }
    }
}
