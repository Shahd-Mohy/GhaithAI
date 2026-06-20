namespace GhaithAI.GaithAI.Application.DTOs.Report
{
    public class CreateFeedbackTagDto
    {
        [Required]
        public FeedbackTagType TagType { get; set; }

        public string? Notes { get; set; }
    }
}
