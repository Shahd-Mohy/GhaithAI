namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface ITranscriptAggregationService
    {
        Task<string> BuildAsync(Guid sessionId);
        Task<string> AggregateTranscriptAsync(Guid sessionId);
    }
}
