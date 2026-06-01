using GhaithAI.API.Data;
using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.API.GaithAI.Infrastructure.Repositories.Class;
using GhaithAI.API.Models;
using GhaithAI.API.Repositories.Class;
using GhaithAI.API.Repositories.Interfaces;

namespace GhaithAI.API.Repositories.UnitWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        public IChatRepository ChatMessages { get; private set; }
        public IRiskRepository RiskEvents { get; private set; }

        public UnitOfWork( ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            ChatMessages = new ChatRepository(_dbContext);
            RiskEvents = new RiskRepository(_dbContext);
        }

        public async Task<int> CompleteAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
