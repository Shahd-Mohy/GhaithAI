namespace GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task AddAsync(T entity);
        void Update(T entity);
        Task Delete(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        IQueryable<T> GetAllQueryableNoTracking();
        IQueryable<T> GetAllQueryableTracking();
        Task AddRangeAsync(IEnumerable<T> entities);
    }
}