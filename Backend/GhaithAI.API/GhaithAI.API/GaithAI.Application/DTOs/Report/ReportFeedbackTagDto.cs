namespace GhaithAI.GaithAI.Application.DTOs.Report
{
    public class ReportFeedbackTagDto
    {
        public Guid Id { get; set; }
        public Guid ClinicalReportId { get; set; }
        public string TagType { get; set; }
        public string CreatorRole { get; set; }
        public string CreatorId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
