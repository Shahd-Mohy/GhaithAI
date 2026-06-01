using GhaithAI.API.Data;
using GhaithAI.API.GaithAI.Infrastructure.Repositories.Class;
using GhaithAI.API.Models;
using GhaithAI.API.Repositories.Interfaces;

namespace GhaithAI.API.Repositories.Class
{
    public class MessageRepository : GenericRepository<ChatMessage>, IMessageRepository
    {
        public MessageRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
