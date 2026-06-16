namespace GhaithAI.API.GaithAI.Application.DTOs.Mood
{
    public class UpdateMoodLogDTO
    {
        public int? MoodScore { get; set; }
        public string? EmotionTags { get; set; }

        public string? Notes { get; set; }
    }
}
