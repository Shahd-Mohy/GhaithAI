using GhaithAI.API.GaithAI.Application.DTOs.Chat;

namespace GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface ILangflowService
    {
        Task<GhaithFinalResultDto> ProcessUserMessageAsync(Guid sessionId, string userMessage);
    }
}
    