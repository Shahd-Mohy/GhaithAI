using GhaithAI.API.GaithAI.Domain.Common;

namespace GhaithAI.API.Models
{
    /// <summary>
    /// Represents an emergency contact for a user.
    /// Sensitive clinical entity requiring full audit trail and soft delete capabilities.
    /// </summary>
    public class EmergencyContact : AuditableEntity<Guid>
    {
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public string FullName { get; set; }

        public string Relationship { get; set; }

        public string PhoneNumber { get; set; }

        public int PriorityOrder { get; set; }
    }
}