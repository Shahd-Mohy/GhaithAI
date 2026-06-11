using GhaithAI.API.DTOs.Emergency;

namespace GhaithAI.API.DTOs.Auth
{
    public class RegisterDTO
    {
        public string FullName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string PhoneNumber { get; set; }

        public string CountryCode { get; set; }

        public string PreferredLanguage { get; set; }

        public bool AcceptedTerms { get; set; }

        public bool AcceptedPrivacyPolicy { get; set; }

        public bool AcceptedAiChat { get; set; }

        public bool AcceptedMoodTracking { get; set; }

        public bool AcceptedDataCollection { get; set; }

        public EmergencyContactDto FirstContact { get; set; }

        public EmergencyContactDto SecondContact { get; set; }

        // Assessment

        public int? Age { get; set; }

        public List<string> Concerns { get; set; } = [];

        public string? SleepQuality { get; set; }

        public string? StressLevel { get; set; }

        public bool HasTherapyHistory { get; set; }

        public bool TakesMedication { get; set; }

        public Gender Gender { get; set; }
    }
}