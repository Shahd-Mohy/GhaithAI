namespace GhaithAI.GaithAI.Application.DTOs.SessionNote
{
    public class NoteResponseDto
    {
        public Guid Id { get; set; }
        public Guid ClinicalSessionId { get; set; }
        public string Content { get; set; }
        public string NoteType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
