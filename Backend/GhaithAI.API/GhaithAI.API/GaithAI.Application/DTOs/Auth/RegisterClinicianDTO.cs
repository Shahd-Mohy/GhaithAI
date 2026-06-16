namespace GhaithAI.GaithAI.Application.DTOs.Auth
{
    public class RegisterClinicianDTO
    {
        public string FullName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string PhoneNumber { get; set; }

        public string CountryCode { get; set; }

        public string PreferredLanguage { get; set; }

        public Gender Gender { get; set; }

        public DoctorType DoctorType { get; set; }

        public string Specialization { get; set; }

        public string Bio { get; set; }

        public int YearsOfExperience { get; set; }

        public IFormFile DocumentsPdf { get; set; }
    }
}
