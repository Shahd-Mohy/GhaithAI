namespace GhaithAI.GaithAI.Domain.Entities
{
    public class Booking : AuditableEntity<Guid>
    {
        public Guid ClinicId { get; set; }
        public string? PatientId { get; set; }
        public Guid? ClinicPatientId { get; set; }

        public DateTime BookingDate { get; set; }
        public TimeSpan SlotTime { get; set; }

        public SessionType SessionType { get; set; } = SessionType.Offline;
        public BookingSource BookingSource { get; set; } = BookingSource.App;
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

        public string Notes { get; set; }

        public DateTime? ConfirmedAt { get; set; }
        public string ConfirmedBy { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancelledBy { get; set; }
        public virtual Clinic Clinic { get; set; }
        public virtual ApplicationUser Patient { get; set; }
        public virtual ClinicPatient ClinicPatient { get; set; }

    }
}
