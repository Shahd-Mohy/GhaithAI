namespace GhaithAI.GaithAI.Application.DTOs.Report
{
    public class ClinicalReportHistoryDto
    {
        public Guid Id { get; set; }
        public Guid ClinicalReportId { get; set; }
        public string OriginalAiContent { get; set; }
        public int TotalChangesCount { get; set; }
        public int? DoctorEditDurationMinutes { get; set; }
        public string ModificationSeverity { get; set; }
        public DateTime CapturedAt { get; set; }
    }
}
