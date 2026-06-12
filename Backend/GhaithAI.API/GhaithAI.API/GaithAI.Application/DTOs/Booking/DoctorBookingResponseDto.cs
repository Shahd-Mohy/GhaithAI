namespace GhaithAI.GaithAI.Application.DTOs.Booking
{
    public class DoctorBookingResponseDto
    {
        public Guid BookingId { get; set; }
        public string PatientName { get; set; }
        public string PatientPhone { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan SlotTime { get; set; }
        public string SessionType { get; set; } // Online / Offline
        public string BookingSource { get; set; } // App / Clinic
        public string Status { get; set; }      // Confirmed / Cancelled...
        public string Notes { get; set; }
    }

   
}
