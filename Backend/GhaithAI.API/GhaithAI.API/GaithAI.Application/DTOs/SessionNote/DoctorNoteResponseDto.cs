namespace GhaithAI.GaithAI.Application.DTOs.SessionNote
{
    public class DoctorNoteResponseDto
    {
        public Guid Id { get; set; }
        public Guid ClinicalSessionId { get; set; }
        public string Content { get; set; }
        public string NoteType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string PatientId { get; set; }
        public string PatientDisplayName { get; set; }
        public DateTime SessionStartedAt { get; set; }
        public string SessionType { get; set; }
    }
}
