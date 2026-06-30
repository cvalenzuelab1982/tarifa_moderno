using Directo.Wari.TarifaEngine.Domain.Common;

namespace Directo.Wari.TarifaEngine.Application.Common.Interfaces
{
    /// <summary>
    /// DbContext de solo lectura para queries optimizadas.
    /// </summary>
    public interface IReadDbContext
    {
        IQueryable<T> Set<T>() where T : BaseEntity;
    }

    /// <summary>
    /// DbContext de escritura con soporte de Unit of Work.
    /// </summary>
    public interface IApplicationDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
