namespace GhaithAI.GaithAI.Application.DTOs.SessionNote
{
    public class AddNoteDto
    {
        public string Content { get; set; }
        public NoteType NoteType { get; set; }
    }

    public class UpdateNoteDto
    {
        public string Content { get; set; }
    }

   
}
