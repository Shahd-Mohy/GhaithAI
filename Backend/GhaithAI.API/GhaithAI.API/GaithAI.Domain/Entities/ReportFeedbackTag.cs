namespace GhaithAI.GaithAI.Domain.Entities
{
    public class ReportFeedbackTag : BaseEntity<Guid>
    {
        public Guid ClinicalReportId { get; set; }
        public FeedbackTagType TagType { get; set; }
        public string CreatorRole { get; set; }
        public string? Notes { get; set; }

        public virtual ClinicalReport ClinicalReport { get; set; }
    }
}
