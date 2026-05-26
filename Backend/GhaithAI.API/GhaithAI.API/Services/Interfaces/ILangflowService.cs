namespace GhaithAI.API.Services.Interfaces
{
    public interface ILangflowService
    {
        Task<string> SendMessageAsync(string userMessage, string sessionId);
    }
}
