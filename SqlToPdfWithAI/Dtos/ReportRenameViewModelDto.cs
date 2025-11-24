namespace SqlToPdfWithAI.Dtos
{
    public class ReportRenameViewModelDto
    {
        public Guid ReportId { get; set; }
        public string ReportName { get; set; } = string.Empty;
    }
}
