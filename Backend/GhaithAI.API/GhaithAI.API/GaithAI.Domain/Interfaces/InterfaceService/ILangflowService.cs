namespace GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface ILangflowService
    {
        Task<string> SendMessageAsync(string userMessage, string sessionId);

    }
}
