using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.API.Models;

namespace GhaithAI.API.Repositories.Interfaces
{
    public interface ISessionRepository : IGenericRepository<ChatSession>
    {
        Task<string> GetLast30MessagesFormattedAsync(Guid sessionId);
    }
}
