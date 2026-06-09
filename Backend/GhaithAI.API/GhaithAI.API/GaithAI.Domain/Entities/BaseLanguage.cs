namespace GhaithAI.GaithAI.Domain.Entities
{
    public class BaseLanguage : BaseEntity<string>
    {
        public string LanguageName { get; set; }
        public virtual ICollection<DoctorLanguage> DoctorLanguages { get; set; } = new List<DoctorLanguage>();
    }
}
