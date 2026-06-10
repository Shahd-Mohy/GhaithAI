namespace GhaithAI.GaithAI.Application.DTOs.DoctorProfile
{
    public class DefaultScheduleDto
    {
        public Guid Id { get; set; }
        public DaysOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpsertScheduleDto
    {
        public DaysOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
    }
}
