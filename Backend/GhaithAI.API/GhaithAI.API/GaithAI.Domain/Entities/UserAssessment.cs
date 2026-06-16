using GhaithAI.API.Models;

namespace GhaithAI.GaithAI.Domain.Entities
{
    public class UserAssessment
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public int? Age { get; set; }

        public string Concerns { get; set; }

        public string SleepQuality { get; set; }

        public string StressLevel { get; set; }

        public bool HasTherapyHistory { get; set; }

        public bool TakesMedication { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; }
    }
}
