
namespace GhaithAI.GaithAI.Domain.Entities
{
    public class DoctorDefaultSchedule : BaseEntity<Guid>
    {
        public Guid ClinicId { get; set; }
        public DaysOfWeek DayOfWeek { get; set; } 
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
        public virtual Clinic Clinic { get; set; }
    }
}
