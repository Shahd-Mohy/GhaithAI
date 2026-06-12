namespace GhaithAI.API.GaithAI.Application.DTOs.SelfHelp
{
    public class AdminSelfHelpGetAllDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public int? DurationMinutes { get; set; }
        public string DifficultyLevel { get; set; }


    }
}
