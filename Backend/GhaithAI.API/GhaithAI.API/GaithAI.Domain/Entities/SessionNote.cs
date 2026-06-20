namespace GhaithAI.GaithAI.Domain.Entities
{
    public class SessionNote : BaseEntity<Guid>
    {
        public Guid ClinicalSessionId { get; set; }
        public string Content { get; set; }
        public NoteType NoteType { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ClinicalSession ClinicalSession { get; set; }
    }
}
