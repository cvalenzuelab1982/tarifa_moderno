using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Directo.Wari.TarifaEngine.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Directo.Wari.TarifaEngine.Infrastructure.Legacy.Persistence
{
    /// <summary>
    /// Implementación de IReadDbContext que lee de la base de datos legacy SQL Server.
    /// Se registra como IReadDbContext durante la Fase 1 (ReadSource = "SqlServer").
    /// Los QueryHandlers no necesitan cambios — solo cambia la fuente de datos via DI.
    /// </summary>
    public class LegacyReadDbContext : IReadDbContext
    {
        private readonly LegacyDbContext _context;

        public LegacyReadDbContext(LegacyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retorna un IQueryable sin tracking para lecturas optimizadas desde SQL Server.
        /// </summary>
        public IQueryable<T> Set<T>() where T : BaseEntity
        {
            return _context.Set<T>().AsNoTracking();
        }
    }
}
