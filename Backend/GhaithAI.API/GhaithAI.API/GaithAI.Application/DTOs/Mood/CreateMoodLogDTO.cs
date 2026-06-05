namespace GhaithAI.API.DTOs.Mood
{
    public class CreateMoodLogDTO
    {
        public int MoodScore { get; set; }

        public string? EmotionTags { get; set; }

        // for future use ISA
        public int StressLevel { get; set; } = 0;
        public int SleepQuality { get; set; } = 0;

        // Lisa Han4of 
        public string? Notes { get; set; }
        public string Source { get; set; } = "manual";
    }
}
