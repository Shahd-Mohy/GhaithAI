namespace GhaithAI.GaithAI.Domain.Entities
{
    public class ClinicalReportHistory : BaseEntity<Guid>
    {
        public Guid ClinicalReportId { get; set; }
        public string OriginalAiContent { get; set; }
        public int TotalChangesCount { get; set; }
        public int DoctorEditDurationMinutes { get; set; }
        public ModificationSeverity ModificationSeverity { get; set; }
        public DateTime CapturedAt { get; set; }

        public virtual ClinicalReport ClinicalReport { get; set; }
    }
}
