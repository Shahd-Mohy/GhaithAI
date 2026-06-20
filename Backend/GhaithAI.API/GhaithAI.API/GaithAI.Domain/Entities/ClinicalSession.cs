namespace GhaithAI.GaithAI.Domain.Entities
{
    public class ClinicalSession : AuditableEntity<Guid>
    {
        public Guid BookingId { get; set; }
        public Guid DoctorId { get; set; }
        public string PatientId { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Pending;
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string? VideoRoomId { get; set; }
        public string? TranscriptRaw { get; set; }
        public string? Notes { get; set; }

        // ✅ أضيف الـ navigation property
        public virtual ICollection<SessionNote> SessionNotes { get; set; } = new List<SessionNote>();
        public virtual Booking Booking { get; set; }
        public virtual DoctorsProfile Doctor { get; set; }
        public virtual ApplicationUser Patient { get; set; }
    }
}
