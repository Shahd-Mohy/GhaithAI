namespace GhaithAI.GaithAI.Domain.Entities
{
    public class DoctorServiceSetting : BaseEntity<Guid>
    {
        public Guid DoctorId { get; set; }

        public decimal FeePerSession { get; set; }
        public int SessionDurationMinutes { get; set; }
        public SessionType AvailableSessionType { get; set; } = SessionType.both;
        public bool IsActive { get; set; } = true;

        public virtual DoctorsProfile Doctor { get; set; }
    }
}
