namespace GhaithAI.GaithAI.Application.DTOs.SelfHelp
{
    public class AdminSelfHelpUpdateDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required, MaxLength(50)]
        public string Type { get; set; }

        [Required]
        public string Description { get; set; }

        [Url]
        public string? ContentUrl { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Required, MaxLength(50)]
        public string DifficultyLevel { get; set; }

        public bool IsActive { get; set; }

        public List<UpdateExerciseTipDto> ExerciseTips { get; set; } = new();
    }

    public class UpdateExerciseTipDto
    {
        public Guid? Id { get; set; } 
        [Required]
        public string Text { get; set; }
    }
}
