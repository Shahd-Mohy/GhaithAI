using System.ComponentModel.DataAnnotations;

namespace GhaithAI.API.Models
{
    public class SelfHelpContent
    {

        [Key]
        public Guid ContentId { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }

        public string? ContentUrl { get; set; }

        public int? DurationMinutes { get; set; }

        public string DifficultyLevel { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserActivity> UserActivities { get; set; }
    }
}