
using GhaithAI.API.Data;
using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository;
using GhaithAI.API.Presistance;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.API.GaithAI.Infrastructure.Repositories.Class
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.AsNoTracking().ToListAsync();
        public IQueryable<T> GetAllQueryableNoTracking() => _dbSet.AsNoTracking().AsQueryable(); // why using AsQueryable here ??
        public IQueryable<T> GetAllQueryableTracking() => _dbSet.AsQueryable();
        public async Task<T?> GetByIdAsync(object id) => await _dbSet.FindAsync(id);
        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public void Update(T entity) => _dbSet.Update(entity);
        public async Task DeleteAsync(object id)
        {
            var item = await _dbSet.FindAsync(id);
            if (item != null)
                _dbSet.Remove(item);
        }
        public async Task AddRangeAsync(IEnumerable<T> entities) =>
            await _context.Set<T>().AddRangeAsync(entities);

    }
}
