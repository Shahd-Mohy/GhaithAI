namespace GhaithAI.GaithAI.Domain.Entities
{
    public class BaseLanguage : BaseEntity<Guid>
    {
        public string LanguageName { get; set; }
        public virtual ICollection<DoctorLanguage> DoctorLanguages { get; set; } = new List<DoctorLanguage>();
    }
}
