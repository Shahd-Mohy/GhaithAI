namespace GhaithAI.GaithAI.Application.DTOs.Admin
{
    public class RejectDoctorDTO
    {
        public Guid DoctorId { get; set; }
        public string? Reason { get; set; }
    }
}
