namespace GhaithAI.GaithAI.Domain.Entities
{
    public class DoctorLanguage : BaseEntity<Guid>
    {
        public Guid DoctorId { get; set; }
        public Guid BaseLanguageId { get; set; }

        public virtual DoctorsProfile Doctor { get; set; }
        public virtual BaseLanguage BaseLanguage { get; set; }
    }
}
