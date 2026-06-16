namespace GhaithAI.GaithAI.Domain.Entities
{
    public class ClinicPatient : BaseEntity<Guid>
    {
        public Guid DoctorId { get; set; }
        public string PatientFullName { get; set; }
        public string PatientPhone { get; set; }
        public string Notes { get; set; }
        public virtual DoctorsProfile Doctor  { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
