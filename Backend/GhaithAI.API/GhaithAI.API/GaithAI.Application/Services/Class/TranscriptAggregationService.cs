using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class TranscriptAggregationService : ITranscriptAggregationService
    {
        public Task<string> BuildAsync(Guid sessionId)
        {
            throw new NotImplementedException("Transcript aggregation will be implemented in the next sprint.");
        }

        public Task<string> AggregateTranscriptAsync(Guid sessionId)
        {
            throw new NotImplementedException("Transcript aggregation will be implemented in the next sprint.");
        }
    }
}
