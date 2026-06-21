namespace GhaithAI.GaithAI.Infrastructure.Repositories.Class
{
    public class SessionTranscriptRepository : GenericRepository<SessionTranscript>, ISessionTranscriptRepository
    {
        public SessionTranscriptRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SessionTranscript>> GetSegmentsBySessionAsync(Guid clinicalSessionId)
        {
            return await _dbSet
                .Where(t => t.ClinicalSessionId == clinicalSessionId)
                .OrderBy(t => t.StartMs)
                .ToListAsync();
        }
    }
}
