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
        public decimal FeePerSession { get; set; }
        public int SessionDurationMinutes { get; set; }
        // åĞÇ ÇáÍŞá íÍÏÏ ÅĞÇ ßÇäÊ ÇáÚíÇÏÉ ÙÇåÑÉ İí äÊÇÆÌ ÇáÈÍË ÇáÚÇãÉ Ãã áÇ¡ æíÃÎĞ false ÇİÊÑÇÖí áÍÏ ãÇ ÇáÃÏãä íæÇİŞ Úáì ÙåæÑåÇ
        public bool IsPublicListed { get; set; }

        public virtual DoctorsProfile Doctor { get; set; }
        public virtual ICollection<ClinicPatient> ClinicPatients { get; set; } = new List<ClinicPatient>();
        public virtual ICollection<DoctorDefaultSchedule> DefaultSchedules { get; set; } = new List<DoctorDefaultSchedule>();
        public virtual ICollection<DoctorCustomSchedule> CustomSchedules { get; set; } = new List<DoctorCustomSchedule>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<DoctorReview> Reviews { get; set; } = new List<DoctorReview>();
    }
}
