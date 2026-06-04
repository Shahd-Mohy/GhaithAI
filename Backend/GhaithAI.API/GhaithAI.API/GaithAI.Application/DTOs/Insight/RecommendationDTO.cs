namespace GhaithAI.API.DTOs.Insight
{
    public class RecommendationDTO
    {
        public string Title { get; set; } = "Today's Suggestion";

        public string Text { get; set; } = string.Empty;

        public string? ActionLabel { get; set; }

        public string? ActionRoute { get; set; }
    }
}
