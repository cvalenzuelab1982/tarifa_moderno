using Directo.Wari.TarifaEngine.Domain.Interfaces;

namespace Directo.Wari.TarifaEngine.Infrastructure.Persistence
{
    /// <summary>
    /// Implementación del Unit of Work.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }

}
