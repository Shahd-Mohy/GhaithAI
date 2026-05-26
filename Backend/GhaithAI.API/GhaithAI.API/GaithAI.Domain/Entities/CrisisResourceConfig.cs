using System.ComponentModel.DataAnnotations;

namespace GhaithAI.API.Models
{
    public class CrisisResourceConfig
    {
        [Key]
        public Guid ResourceId { get; set; }

        public string CountryCode { get; set; }

        public Country Country { get; set; }

        public string EmergencyNumber { get; set; }

        public string CrisisHotline { get; set; }

        public string CrisisTextLine { get; set; }

        public string? WebsiteUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}