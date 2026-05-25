namespace GhaithAI.API.Models
{
    public class Country
    {
        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        public string IsoCode { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }

        public ICollection<CrisisResourceConfig> CrisisResourceConfigs { get; set; }
    }
}