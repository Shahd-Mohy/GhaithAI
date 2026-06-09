namespace GhaithAI.GaithAI.Domain.Entities
{
    public class DoctorReview : BaseEntity<Guid>
    {
        public Guid ClinicId { get; set; }
        public string PatientId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }

        public virtual Clinic Clinic { get; set; }
        public virtual ApplicationUser Patient { get; set; }

    }
}
