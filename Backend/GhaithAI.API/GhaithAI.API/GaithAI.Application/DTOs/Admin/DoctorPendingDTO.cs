namespace GhaithAI.GaithAI.Application.DTOs.Admin
{
    public class DoctorPendingDTO
    {
        public Guid Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Specialization { get; set; }

        public int YearsOfExperience { get; set; }

        public string DocumentsPdfUrl { get; set; }

        public ApprovalStatus ApprovalStatus { get; set; }
    }
}
