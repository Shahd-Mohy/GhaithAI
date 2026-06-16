namespace GhaithAI.API.GaithAI.Application.DTOs.Mood
{
    public class MoodLogCreatedDTO
    {
        public Guid MoodLogId { get; set; }
        public DateTime LoggedAt { get; set; }
        public string MoodLabel { get; set; } = string.Empty;
    }
}
