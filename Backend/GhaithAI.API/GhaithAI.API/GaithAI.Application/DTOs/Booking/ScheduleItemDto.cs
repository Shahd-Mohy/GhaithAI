namespace GhaithAI.GaithAI.Application.DTOs.Booking
{
    public class ScheduleItemDto
    {
        public Guid BookingId { get; set; }
        public string PatientName { get; set; } 
        public string SessionType { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
    }
}
