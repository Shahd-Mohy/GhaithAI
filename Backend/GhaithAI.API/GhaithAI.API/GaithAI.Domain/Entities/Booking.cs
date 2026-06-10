namespace GhaithAI.GaithAI.Domain.Entities
{
    public class Booking : AuditableEntity<Guid>
    {
        public Guid DoctorId { get; set; } 
        public string? PatientId { get; set; }
        public Guid? ClinicPatientId { get; set; }

        public DateTime BookingDate { get; set; }
        public TimeSpan SlotTime { get; set; }
        public AttendanceType SessionType { get; set; } = AttendanceType.offline;
        public BookingSource BookingSource { get; set; } = BookingSource.App;
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
        public string Notes { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string ConfirmedBy { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancelledBy { get; set; }

        public virtual DoctorsProfile Doctor { get; set; }
        public virtual ApplicationUser Patient { get; set; }
        public virtual ClinicPatient ClinicPatient { get; set; }

    }
}
