using GhaithAI.API.GaithAI.Domain.Common;

namespace GhaithAI.GaithAI.Domain.Entities
{
    public class ReportFeedbackTag : AuditableEntity<Guid>
    {
        public Guid Id { get; set; }

        public Guid ClinicalReportId { get; set; }
        public ClinicalReport ClinicalReport { get; set; }

        public FeedbackTagType TagType { get; set; }

        public string CreatorRole { get; set; } // "Doctor" أو "Admin"
        public string CreatorId { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
