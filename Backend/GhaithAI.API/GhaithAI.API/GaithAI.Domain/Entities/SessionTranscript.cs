namespace GhaithAI.GaithAI.Domain.Entities
{
    public class SessionTranscript : BaseEntity<Guid>
    {
        public Guid ClinicalSessionId { get; set; }
        public SpeakerRole Speaker { get; set; }
        public string Content { get; set; }
        public int StartMs { get; set; }
        public int EndMs { get; set; }
        public decimal? ConfidenceScore { get; set; }
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; }

        public virtual ClinicalSession ClinicalSession { get; set; }
    }
}
