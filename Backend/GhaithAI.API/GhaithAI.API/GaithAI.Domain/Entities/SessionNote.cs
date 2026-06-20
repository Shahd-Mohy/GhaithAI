namespace GhaithAI.GaithAI.Domain.Entities
{
    public class SessionNote : AuditableEntity<Guid>
    {
        public Guid ClinicalSessionId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string CreatedByUserId { get; set; } = string.Empty;
        public virtual ClinicalSession ClinicalSession { get; set; }
    }
}
