namespace GhaithAI.GaithAI.Application.DTOs.DoctorProfile
{
    public class DoctorClinicProfileDto
    {
        public string PracticeType { get; set; }
        public string DisplayName { get; set; }
        public string ProfessionalTitle { get; set; }
        public int YearsOfExperience { get; set; }
        public string Bio { get; set; }
        public string ClinicName { get; set; }

        public string ClinicAddress { get; set; }
        public string City { get; set; }
        public string CountryCode { get; set; }
        public string Phone { get; set; }
        public string ContactEmail { get; set; }
        public bool IsPublicListed { get; set; }
        public decimal FeePerSession { get; set; }
        public int SessionDurationMinutes { get; set; }
        public SessionType AvailableSessionType { get; set; }
        public List<string> Specialties { get; set; } = new();
        public List<string> Languages { get; set; } = new();
        public List<Guid> SpecialtyIds { get; set; } = new();
        public List<Guid> LanguageIds { get; set; } = new();
        public List<DefaultScheduleDto> WeeklySchedule { get; set; } = new();
        public List<CustomScheduleDto> CustomSchedules { get; set; } = new();
    }
}
