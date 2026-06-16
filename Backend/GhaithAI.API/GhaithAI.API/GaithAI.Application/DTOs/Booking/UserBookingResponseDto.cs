namespace GhaithAI.GaithAI.Application.DTOs.Booking
{
    public class UserBookingResponseDto
    {
        public Guid BookingId { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSpecialization { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan SlotTime { get; set; }
        public string DisplayTime { get; set; }
        public string SessionType { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public bool CanCancel { get; set; }
    }
}