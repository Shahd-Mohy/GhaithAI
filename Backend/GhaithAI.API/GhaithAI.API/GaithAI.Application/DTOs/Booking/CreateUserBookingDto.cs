namespace GhaithAI.GaithAI.Application.DTOs.Booking
{
    public class CreateUserBookingDto
    {
        public Guid DoctorId { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan SlotTime { get; set; }
        public AttendanceType SessionType { get; set; } = AttendanceType.offline;
        public string BookingNotes { get; set; } = "";
    }
}