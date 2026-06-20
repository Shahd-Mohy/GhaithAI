namespace GhaithAI.GaithAI.Domain.Entities
{
    public class ReportSection : BaseEntity<Guid>
    {
        public Guid ClinicalReportId { get; set; }
        public SectionType SectionType { get; set; }
        public string Title { get; set; }
        public string AiContent { get; set; }
        public string? DoctorContent { get; set; }
        public int OrderIndex { get; set; }
        public bool IsEdited { get; set; } = false;

        public virtual ClinicalReport ClinicalReport { get; set; }
    }
}
