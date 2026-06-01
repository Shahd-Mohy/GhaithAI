using GhaithAI.API.Repositories.Interfaces;

namespace GhaithAI.API.Repositories.UnitWork
{
    public interface IUnitOfWork : IDisposable
    {
        IChatRepository Chat { get; }
        IMessageRepository Message { get; }
        IRiskRepository Risk { get; }

        Task<int> CompleteAsync();
        int Complete();
    }
}
