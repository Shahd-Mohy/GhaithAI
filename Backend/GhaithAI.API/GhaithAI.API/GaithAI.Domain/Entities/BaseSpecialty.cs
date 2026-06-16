namespace GhaithAI.GaithAI.Domain.Entities
{
    public class BaseSpecialty : BaseEntity<Guid>
    {
        public string SpecialtyName { get; set; }
        public virtual ICollection<DoctorSpecialty> DoctorSpecialties { get; set; } = new List<DoctorSpecialty>();
    }
}
