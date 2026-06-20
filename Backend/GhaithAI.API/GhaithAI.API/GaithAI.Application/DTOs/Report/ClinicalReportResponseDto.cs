namespace GhaithAI.GaithAI.Application.DTOs.Report
{
    public class ClinicalReportResponseDto
    {
        public Guid Id { get; set; }
        public Guid ClinicalSessionId { get; set; }
        public string ReportType { get; set; }
        public string Status { get; set; }
        public string FinalContent { get; set; }
        public string? DoctorNotes { get; set; }
        public string AiModelVersion { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsAiGenerated { get; set; }
        public string? PdfUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<ReportSectionDto> Sections { get; set; } = new();
    }
}
