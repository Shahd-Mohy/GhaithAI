namespace GhaithAI.GaithAI.Application.DTOs.ClinicalSession
{
    public class StartSessionDto
    {
        public Guid BookingId { get; set; }
        public SessionType SessionType { get; set; }
        public string Provider { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? VideoRoomId { get; set; }
        public string? VideoRoomUrl { get; set; }
    }
}
