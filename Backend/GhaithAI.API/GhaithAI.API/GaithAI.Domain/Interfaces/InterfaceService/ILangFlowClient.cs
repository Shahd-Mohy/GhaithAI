namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface ILangFlowClient
    {
        Task<string> RunFlowAsync(string flowId, object payload, CancellationToken ct = default);
    }
}
