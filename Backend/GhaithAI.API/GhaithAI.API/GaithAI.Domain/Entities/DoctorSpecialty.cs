namespace GhaithAI.GaithAI.Domain.Entities
{
    public class DoctorSpecialty : BaseEntity<Guid>
    {
        public Guid DoctorId { get; set; }
        public Guid SpecialtyId { get; set; }
        public virtual DoctorsProfile Doctor { get; set; }
        public virtual BaseSpecialty BaseSpecialty { get; set; }
    }
}
