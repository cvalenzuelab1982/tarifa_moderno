using Directo.Wari.TarifaEngine.Domain.Common;
using Directo.Wari.TarifaEngine.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Directo.Wari.TarifaEngine.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación genérica del repositorio base.
    /// </summary>
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync([id], cancellationToken);
        }

        public virtual async Task<List<T>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public virtual async Task<int> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity.Id;
        }

        public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
