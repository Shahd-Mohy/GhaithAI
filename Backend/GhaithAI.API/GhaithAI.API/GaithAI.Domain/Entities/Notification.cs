namespace GhaithAI.GaithAI.Domain.Entities
{
    public class Notification : BaseEntity<Guid>
    {
        public string UserId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
