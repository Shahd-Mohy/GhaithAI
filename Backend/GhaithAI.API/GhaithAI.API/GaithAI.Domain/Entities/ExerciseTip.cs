namespace GhaithAI.GaithAI.Domain.Entities
{
    public class ExerciseTip : AuditableEntity<Guid>
    {
        public string Text { get; set; }
        
        public Guid SelfHelpContentId { get; set; }
        public SelfHelpContent SelfHelpContent { get; set; }
    }
}
