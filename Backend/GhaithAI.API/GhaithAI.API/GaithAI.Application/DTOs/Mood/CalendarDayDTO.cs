namespace GhaithAI.API.GaithAI.Application.DTOs.Mood
{
    public class CalendarDayDTO
    {
        public string Date { get; set; } = string.Empty;

        public int MoodScore { get; set; }

        public string MoodLabel { get; set; } = string.Empty;

        public string MoodBadge { get; set; } = string.Empty;

        public List<string> EmotionTags { get; set; } = new();
    }
}
