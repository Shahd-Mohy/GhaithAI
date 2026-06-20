namespace GhaithAI.GaithAI.Application.DTOs.Report
{
    public class ReportSectionDto
    {
        public Guid Id { get; set; }
        public string SectionType { get; set; }
        public string Title { get; set; }
        public string AiContent { get; set; }
        public string? DoctorContent { get; set; }
        public int OrderIndex { get; set; }
        public bool IsEdited { get; set; }

        // الـ content الفعلي اللي يتعرض للدكتور: DoctorContent لو موجود وإلا AiContent
        public string DisplayContent => string.IsNullOrEmpty(DoctorContent) ? AiContent : DoctorContent;
    }
}
