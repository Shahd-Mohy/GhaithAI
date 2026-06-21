namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface ITranscriptionService
    {
        Task ProcessAsync(Guid clinicalSessionId, string audioFilePath);
    }
}
