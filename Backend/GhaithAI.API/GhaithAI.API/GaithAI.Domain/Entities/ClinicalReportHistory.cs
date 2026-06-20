using GhaithAI.API.GaithAI.Domain.Common;

namespace GhaithAI.GaithAI.Domain.Entities
{
    public class ClinicalReportHistory : AuditableEntity<Guid>
    {
        public Guid Id { get; set; }

        public Guid ClinicalReportId { get; set; }
        public ClinicalReport ClinicalReport { get; set; }

        // نسخة كاملة من ناتج الـ AI الخام وقت التوليد — مرجع ثابت لا يتغير
        public string OriginalAiContent { get; set; }

        public int TotalChangesCount { get; set; } = 0;

        public int? DoctorEditDurationMinutes { get; set; }

        public ModificationSeverity ModificationSeverity { get; set; } = ModificationSeverity.Minor;

        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    }

}
