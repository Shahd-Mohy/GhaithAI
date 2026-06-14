using GhaithAI.GaithAI.Application.DTOs.ClinicPatient;

namespace GhaithAI.GaithAI.Application.DTOs.Booking
{
    public class CreateClinicBookingDto
    {
        public Guid? ClinicPatientId { get; set; }
        public CreateClinicPatientDto? NewPatientInfo { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan SlotTime { get; set; }
        public AttendanceType SessionType { get; set; } = AttendanceType.offline;
        public string BookingNotes { get; set; } = "";
    }
}
