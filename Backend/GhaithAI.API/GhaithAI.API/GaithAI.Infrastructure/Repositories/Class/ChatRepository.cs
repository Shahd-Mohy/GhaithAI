using GhaithAI.API.Data;
using GhaithAI.API.GaithAI.Infrastructure.Repositories.Class;
using GhaithAI.API.Models;
using GhaithAI.API.Repositories.Interfaces;

namespace GhaithAI.API.Repositories.Class
{
    public class ChatRepository : GenericRepository<ChatSession>, IChatRepository
    {
        public ChatRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
