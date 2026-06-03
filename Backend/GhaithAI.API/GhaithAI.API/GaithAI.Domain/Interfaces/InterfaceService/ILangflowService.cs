//using GhaithAI.API.GaithAI.Application.DTOs.Chat;

//namespace GhaithAI.API.Services.Interfaces
//{
//    public interface ILangflowService
//    {
//        Task<string> SendMessageAsync(string userMessage, string sessionId);
//    }
//}


//namespace GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceService
//{
//    public interface ILangflowService
//    {
//        Task<GhaithFinalResultDto> ProcessUserMessageAsync(Guid sessionId, string userMessage);
//    }
//}
using GhaithAI.API.GaithAI.Application.DTOs.Chat;

namespace GhaithAI.API.Services.Interfaces
{
    public interface ILangflowService
    {
        Task<string> SendMessageAsync(
            string userMessage,
            string sessionId);

        Task<GhaithFinalResultDto> ProcessUserMessageAsync(
            Guid sessionId,
            string userMessage);
    }
}
