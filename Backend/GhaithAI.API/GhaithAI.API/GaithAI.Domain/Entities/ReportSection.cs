using GhaithAI.API.GaithAI.Domain.Common;
using GhaithAI.GaithAI.Domain.Enums;

namespace GhaithAI.GaithAI.Domain.Entities
{
    public class ReportSection : AuditableEntity<Guid>
    {
        public Guid Id { get; set; }

        public Guid ClinicalReportId { get; set; }
        public ClinicalReport ClinicalReport { get; set; }

        public SectionType SectionType { get; set; }

        public string Title { get; set; }

        // محتوى الـ AI الأصلي — لا يتغير أبداً بعد التوليد
        public string AiContent { get; set; }

        // تعديل الدكتور — null لو لسه ما اتعدلش
        public string? DoctorContent { get; set; }

        public int OrderIndex { get; set; }

        public bool IsEdited { get; set; } = false;
    }
}
