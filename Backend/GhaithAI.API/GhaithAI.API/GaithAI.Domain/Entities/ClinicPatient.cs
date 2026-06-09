namespace GhaithAI.GaithAI.Domain.Entities
{
    public class ClinicPatient : AuditableEntity<Guid>
    {
        public Guid ClinicId { get; set; }
        public string PatientFullName { get; set; }
        public string PatientPhone { get; set; }
        public string Notes { get; set; }
        public virtual Clinic Clinic { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
