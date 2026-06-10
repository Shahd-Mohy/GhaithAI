namespace GhaithAI.GaithAI.Application.DTOs.DoctorProfile
{
    public class PublicDoctorCardDto
    {
        public Guid DoctorId { get; set; }
        public string DisplayName { get; set; }
        public string ProfessionalTitle { get; set; }
        public float AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int YearsOfExperience { get; set; }
        public string City { get; set; }
        public string CountryCode { get; set; }
        public string Bio { get; set; }
        public decimal FeePerSession { get; set; }
        public SessionType AvailableSessionType { get; set; }
        public List<string> Specialties { get; set; } = new();
        public List<string> Languages { get; set; } = new();
        public string NextAvailableSlot { get; set; }  
    }
}
