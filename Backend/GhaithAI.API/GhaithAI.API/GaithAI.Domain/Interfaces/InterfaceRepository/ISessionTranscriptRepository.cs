namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceRepository
{
    public interface ISessionTranscriptRepository : IGenericRepository<SessionTranscript>
    {
        Task<IEnumerable<SessionTranscript>> GetSegmentsBySessionAsync(Guid clinicalSessionId);
    }
}
