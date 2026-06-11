namespace GhaithAI.GaithAI.Application.DTOs.SelfHelp
{
    public class AdminSelfHelpDetailsDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string? ContentUrl { get; set; }
        public int? DurationMinutes { get; set; }
        public string DifficultyLevel { get; set; }
        public bool IsActive { get; set; }

        // 📝 لستة النصائح والتمارين المربوطة بالمقال ده
        public List<ExerciseTipDto> ExerciseTips { get; set; } = new();
    }
    public class ExerciseTipDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
    }
}
