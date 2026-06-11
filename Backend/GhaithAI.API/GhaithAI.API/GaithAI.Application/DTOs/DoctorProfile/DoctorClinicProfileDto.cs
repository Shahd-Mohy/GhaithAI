namespace GhaithAI.GaithAI.Application.DTOs.DoctorProfile
{
    public class DoctorClinicProfileDto
    {
        public string ClinicName { get; set; }
        public string DisplayName { get; set; }        
        public string ProfessionalTitle { get; set; }
        public int YearsOfExperience { get; set; }
        public string Bio { get; set; }
        public bool IsPublicListed { get; set; }

        public string ClinicAddress { get; set; }
        public string City { get; set; }
        public string CountryCode { get; set; }
        public string Phone { get; set; }
        public string ContactEmail { get; set; }

        public List<SpecialtyItemDto> Specialties { get; set; } = new();
        public List<LanguageItemDto> Languages { get; set; } = new();

        public decimal FeePerSession { get; set; }
        public int SessionDurationMinutes { get; set; }
        public SessionType AvailableSessionType { get; set; }

        public List<DefaultScheduleDto> WeeklySchedule { get; set; } = new();
    }
}
