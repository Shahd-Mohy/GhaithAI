using GhaithAI.API.Data;
using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.API.Repositories.Interfaces
{
    public interface IChatRepository : IGenericRepository<ChatMessage>
    {
        Task<string> GetLast30MessagesFormattedAsync(Guid sessionId);
    }
}
