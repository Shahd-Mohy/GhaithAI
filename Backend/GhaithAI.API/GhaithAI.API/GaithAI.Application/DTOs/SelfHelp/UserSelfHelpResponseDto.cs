namespace GhaithAI.API.GaithAI.Application.DTOs.SelfHelp
{
    public class UserSelfHelpResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string? ContentUrl { get; set; }
        public int? DurationMinutes { get; set; }
        public string DifficultyLevel { get; set; }
    }
}
