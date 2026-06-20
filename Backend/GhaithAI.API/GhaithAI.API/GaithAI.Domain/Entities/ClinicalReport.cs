using GhaithAI.API.GaithAI.Domain.Common;
using GhaithAI.GaithAI.Domain.Enums;

namespace GhaithAI.GaithAI.Domain.Entities
{
    public class ClinicalReport : AuditableEntity<Guid>
    {
        public Guid Id { get; set; }

        public Guid ClinicalSessionId { get; set; }
        public ClinicalSession ClinicalSession { get; set; }

        public string ReportType { get; set; } = "Standard";

        public ReportStatus Status { get; set; } = ReportStatus.Draft;

        // الناتج الخام من LangFlow قبل أي تعديل
        public string AiDraftJson { get; set; }

        // النص النهائي بعد دمج تعديلات الدكتور (snapshot كامل)
        public string FinalContent { get; set; }

        public string? DoctorNotes { get; set; }

        public string AiModelVersion { get; set; }

        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        public bool IsAiGenerated { get; set; } = true;

        public string? PdfUrl { get; set; }

        public ICollection<ReportSection> ReportSections { get; set; } = new List<ReportSection>();

        // علاقة 1:1 إجبارية مع الـ History (راجع الملاحظة في الـ Configuration)
        public ClinicalReportHistory ClinicalReportHistory { get; set; }

        public ICollection<ReportFeedbackTag> FeedbackTags { get; set; } = new List<ReportFeedbackTag>();
    }
}
