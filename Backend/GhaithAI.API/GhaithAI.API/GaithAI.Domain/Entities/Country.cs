using GhaithAI.API.GaithAI.Domain.Common;

namespace GhaithAI.API.Models
{
    /// <summary>
    /// Represents a country reference entity.
    /// Global lookup table with string-based country code as primary key.
    /// </summary>
    public class Country : BaseEntity<string>
    {
        public string CountryName { get; set; }

        public string IsoCode { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }

        public ICollection<CrisisResourceConfig> CrisisResourceConfigs { get; set; }
    }
}