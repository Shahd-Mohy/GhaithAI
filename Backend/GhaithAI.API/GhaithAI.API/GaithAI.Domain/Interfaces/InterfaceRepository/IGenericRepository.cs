namespace GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Delete(T entity);
        Task DeleteAsync(object id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(object id);
        IQueryable<T> GetAllQueryableNoTracking();
        IQueryable<T> GetAllQueryableTracking();

    }
}