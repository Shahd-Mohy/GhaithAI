global using System.ComponentModel.DataAnnotations;

namespace GhaithAI.API.GaithAI.Application.DTOs.SelfHelp
{
    public class AdminSelfHelpSaveDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required, MaxLength(50)] 
        public string Type { get; set; }

        [Required]
        public string Description { get; set; }

        [Url]
        public string? ContentUrl { get; set; }

        public int? DurationMinutes { get; set; }

        [Required, MaxLength(50)] 
        public string DifficultyLevel { get; set; }

        public bool IsActive { get; set; } = true;

        public List<CreateExerciseTipDto> ExerciseTips { get; set; } = new();
    }
    public class CreateExerciseTipDto
    {
        [Required]
        public string Text { get; set; }
    }
}
