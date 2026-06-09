namespace GhaithAI.GaithAI.Domain.Entities
{
    public class DoctorCustomSchedule : BaseEntity<Guid>    
    {
        public Guid ClinicId { get; set; }
        public DateTime CustomDate { get; set; } 
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsOffDay { get; set; }
        public virtual Clinic Clinic { get; set; }
    }
}
