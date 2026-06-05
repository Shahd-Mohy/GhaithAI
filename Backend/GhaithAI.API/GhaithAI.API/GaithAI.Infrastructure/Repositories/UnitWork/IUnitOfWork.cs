using GhaithAI.API.Repositories.Interfaces;

namespace GhaithAI.API.Repositories.UnitWork
{
    public interface IUnitOfWork : IDisposable
    {
        ISessionRepository Session { get; }
        IMessageRepository Message { get; }
        IRiskRepository Risk { get; }
        ISelfHelpRepository SelfHelp { get; }

        Task<int> CompleteAsync();
        int Complete();
    }
}
