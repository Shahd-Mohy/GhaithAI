namespace GhaithAI.GaithAI.Application.DTOs.ClinicalSession
{
    public class SessionResponseDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid DoctorId { get; set; }
        public string PatientId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? DurationMinutes { get; set; }
        public string Status { get; set; }
        public string SessionType { get; set; }
        public string? ChiefComplaint { get; set; }
        public string Provider { get; set; }
        public string? VideoRoomId { get; set; }
        public string? VideoRoomUrl { get; set; }
    }
}
