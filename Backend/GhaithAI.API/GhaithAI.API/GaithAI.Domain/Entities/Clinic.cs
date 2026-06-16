namespace GhaithAI.GaithAI.Domain.Entities
{
    public class Clinic : AuditableEntity<Guid>
    {
        public Guid DoctorId { get; set; }
        public string ClinicName { get; set; }
        public string ClinicAddress { get; set; }
        public string City { get; set; }
        public string CountryCode { get; set; }
        public string Phone { get; set; }
        public string ContactEmail { get; set; }

        public bool IsPublicListed { get; set; }
        public virtual DoctorsProfile Doctor { get; set; }
       
    }
}
