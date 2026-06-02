namespace GhaithAI.API.DTOs.Mood
{
    public class MoodInsightDTO
    {
        public string BestDayTitle { get; set; } = "Best Day";
        public string BestDayText { get; set; } = string.Empty;  

        public string WatchOutTitle { get; set; } = "Watch Out";
        public string WatchOutText { get; set; } = string.Empty; 

        public string ConnectionTitle { get; set; } = "Connection";
        public string ConnectionText { get; set; } = string.Empty; 
        public List<string> TopEmotions { get; set; } = new();
    }
}
