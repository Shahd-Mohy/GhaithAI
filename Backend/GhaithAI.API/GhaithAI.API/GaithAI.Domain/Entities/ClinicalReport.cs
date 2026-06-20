namespace GhaithAI.GaithAI.Domain.Entities
{
    public class ClinicalReport : BaseEntity<Guid>
    {
        public Guid ClinicalSessionId { get; set; }
        public string ReportType { get; set; }
        public ReportStatus Status { get; set; } = ReportStatus.Draft;
        public string AiDraftJson { get; set; }
        public string FinalContent { get; set; }
        public string? DoctorNotes { get; set; }
        public string AiModelVersion { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsAiGenerated { get; set; } = true;
        public string? PdfUrl { get; set; }

        public virtual ClinicalSession ClinicalSession { get; set; }
    }
}
