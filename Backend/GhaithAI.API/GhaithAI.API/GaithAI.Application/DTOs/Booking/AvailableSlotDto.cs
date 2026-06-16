namespace GhaithAI.GaithAI.Application.DTOs.Booking
{
    public class AvailableSlotDto
    {
        public TimeSpan SlotTime { get; set; }
        public string DisplayTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}