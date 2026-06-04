namespace GhaithAI.API.GaithAI.Application.DTOs.Journal
{
    public class JournalCreatedDTO
    {
        public Guid JournalId { get; set; }

        public DateTime CreatedAt { get; set; }

        public int WordCount { get; set; }
    }
}
