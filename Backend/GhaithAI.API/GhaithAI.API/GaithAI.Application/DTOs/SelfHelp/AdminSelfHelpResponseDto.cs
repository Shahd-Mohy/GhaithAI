namespace GhaithAI.API.GaithAI.Application.DTOs.SelfHelp
{
    public class AdminSelfHelpResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string? ContentUrl { get; set; }
        public int? DurationMinutes { get; set; }
        public string DifficultyLevel { get; set; }
        public bool IsActive { get; set; }

        //  ›«’Ì· «·‹ Audit «·„Â„… ··‹ Admin
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
