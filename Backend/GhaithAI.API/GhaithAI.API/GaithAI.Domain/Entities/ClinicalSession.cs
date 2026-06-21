
﻿namespace GhaithAI.GaithAI.Domain.Entities
{
    public class ClinicalSession : BaseEntity<Guid>
    {
        public Guid BookingId { get; set; }
        public Guid DoctorId { get; set; }
        public string PatientId { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? DurationMinutes { get; set; }

        public ClinicalSessionStatus Status { get; set; } = ClinicalSessionStatus.InProgress;
        public SessionType SessionType { get; set; }
        public string? ChiefComplaint { get; set; }
        public string Provider { get; set; }
        public string? VideoRoomId { get; set; }
        public string? VideoRoomUrl { get; set; }
        public TranscriptionStatus? TranscriptionStatus { get; set; }

        public virtual Booking Booking { get; set; }
        public virtual DoctorsProfile Doctor { get; set; }
        public virtual ApplicationUser Patient { get; set; }
        public virtual ICollection<SessionNote> Notes { get; set; } = new List<SessionNote>();
    }
}
