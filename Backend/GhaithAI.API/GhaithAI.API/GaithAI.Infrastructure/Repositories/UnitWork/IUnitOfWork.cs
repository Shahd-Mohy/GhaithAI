using GhaithAI.API.Repositories.Interfaces;

namespace GhaithAI.API.Repositories.UnitWork
{
    /// <summary>
    /// Unit of Work interface providing centralized access to all repositories.
    /// Manages transaction boundaries and ensures ACID compliance across repository operations.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IChatRepository Chat { get; }
        IMessageRepository Message { get; }

        /// <summary>
        /// Asynchronously commits all changes made in the context to the database.
        /// Handles audit trails, soft deletes, and timestamp management via DbContext interceptors.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> CompleteAsync();

        /// <summary>
        /// Synchronously commits all changes made in the context to the database.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        int Complete();
    }
}
