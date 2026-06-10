global using GhaithAI.GaithAI.Domain.Enums;
using System;

namespace GhaithAI.GaithAI.Domain.Entities
{
    public class DoctorsProfile : AuditableEntity<Guid>
    {
        public string FullName { get; set; }

        public DoctorType DoctorType { get; set; } 

        public string Specialization { get; set; }

        public string Bio { get; set; }

        public int YearsOfExperience { get; set; }

        public string DocumentsPdfUrl { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

        public string RejectionReason { get; set; }

        public float AverageRating { get; set; }

        public string UserId { get; set; } 

        public virtual ApplicationUser User { get; set; }

        public virtual Clinic Clinic { get; set; }
        public virtual DoctorServiceSetting ServiceSetting { get; set; }
        public virtual ICollection<DoctorSpecialty> DoctorSpecialties { get; set; } = new List<DoctorSpecialty>();
        public virtual ICollection<DoctorLanguage> DoctorLanguages { get; set; } = new List<DoctorLanguage>();
        public virtual ICollection<ClinicPatient> ClinicPatients { get; set; } = new List<ClinicPatient>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<DoctorReview> Reviews { get; set; } = new List<DoctorReview>();

        public virtual ICollection<DoctorDefaultSchedule> DefaultSchedules { get; set; } = new List<DoctorDefaultSchedule>();
        public virtual ICollection<DoctorCustomSchedule> CustomSchedules { get; set; } = new List<DoctorCustomSchedule>();
    }
}
