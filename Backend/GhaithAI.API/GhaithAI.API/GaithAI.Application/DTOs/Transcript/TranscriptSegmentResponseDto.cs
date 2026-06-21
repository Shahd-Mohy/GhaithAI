namespace GhaithAI.GaithAI.Application.DTOs.Transcript
{
    public class TranscriptSegmentResponseDto
    {
        public Guid Id { get; set; }
        public Guid ClinicalSessionId { get; set; }
        public string Speaker { get; set; }
        public string Content { get; set; }
        public int StartMs { get; set; }
        public int EndMs { get; set; }
        public decimal? ConfidenceScore { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
    }
}
