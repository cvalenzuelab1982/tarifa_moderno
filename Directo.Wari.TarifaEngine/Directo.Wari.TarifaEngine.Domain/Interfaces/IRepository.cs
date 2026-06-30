using Directo.Wari.TarifaEngine.Domain.Common;

namespace Directo.Wari.TarifaEngine.Domain.Interfaces
{
    /// <summary>
    /// Repositorio genérico base.
    /// </summary>
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<T>> ListAllAsync(CancellationToken cancellationToken = default);
        Task<int> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Unit of Work para coordinar transacciones.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
