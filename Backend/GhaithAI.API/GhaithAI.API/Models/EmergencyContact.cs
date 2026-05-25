using System.ComponentModel.DataAnnotations;

namespace GhaithAI.API.Models
{
    public class EmergencyContact
    {
        [Key]
        public Guid ContactId { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public string FullName { get; set; }

        public string Relationship { get; set; }

        public string PhoneNumber { get; set; }

        public int PriorityOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}