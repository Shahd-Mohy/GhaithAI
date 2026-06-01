namespace GhaithAI.API.Constants
{
    public static class EmotionTypes
    {
        public const string Anxious = "Anxious";
        public const string Happy = "Happy";
        public const string Sad = "Sad";
        public const string Stressed = "Stressed";
        public const string Calm = "Calm";
        public const string Frustrated = "Frustrated";
        public const string Hopeful = "Hopeful";
        public const string Tired = "Tired";
        public const string Energetic = "Energetic";
        public const string Lonely = "Lonely";
        public const string Grateful = "Grateful";
        public const string Overwhelmed = "Overwhelmed";

        public static readonly List<string> All = new()
        {
            Anxious, Happy, Sad, Stressed, Calm,
            Frustrated, Hopeful, Tired, Energetic,
            Lonely, Grateful, Overwhelmed
        };

        public static List<string> Parse(string? tags)
        {
            if (string.IsNullOrWhiteSpace(tags)) return new List<string>();
            return tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(t => t.Trim())
                       .ToList();
        }
    }
}