namespace GhaithAI.API.DTOs.Mood
{
    public class MoodHistoryDTO
    {
        public Guid MoodLogId { get; set; }
        public int MoodScore { get; set; }

        public string MoodLabel { get; set; } = string.Empty;

        public string MoodBadge { get; set; } = string.Empty;

        public List<string> EmotionTags { get; set; } = new();

        public string Date { get; set; } = string.Empty;

        public DateTime LoggedAt { get; set; }
    }
}
