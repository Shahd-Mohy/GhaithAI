namespace GhaithAI.GaithAI.Application.DTOs.Admin
{
    public class DoctorProfileDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string DoctorType { get; set; }
        public string Specialization { get; set; }
        public string Bio { get; set; }
        public int YearsOfExperience { get; set; }
        public string DocumentsPdfUrl { get; set; }
        public string ApprovalStatus { get; set; }
        public string? RejectionReason { get; set; }
        public float AverageRating { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? ProfilePicture { get; set; }
    }
}