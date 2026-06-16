using GhaithAI.API.GaithAI.Domain.Common;

namespace GhaithAI.API.Models
{
    /// <summary>
    /// Represents crisis resource configuration for a country.
    /// Immutable reference data capturing point-in-time crisis resources and contact information.
    /// </summary>
    public class CrisisResourceConfig : BaseEntity<Guid>
    {
        public string CountryCode { get; set; }

        public Country Country { get; set; }

        public string EmergencyNumber { get; set; }

        public string CrisisHotline { get; set; }

        public string CrisisTextLine { get; set; }

        public string? WebsiteUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }
}