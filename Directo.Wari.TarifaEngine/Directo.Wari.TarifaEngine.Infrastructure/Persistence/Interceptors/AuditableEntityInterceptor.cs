using Directo.Wari.TarifaEngine.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Directo.Wari.TarifaEngine.Infrastructure.Persistence.Interceptors
{
    /// <summary>
    /// Interceptor que actualiza automáticamente los campos de auditoría.
    /// </summary>
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                UpdateAuditableEntities(eventData.Context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void UpdateAuditableEntities(DbContext context)
        {
            var entries = context.ChangeTracker.Entries<IAuditableEntity>();

            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).CurrentValue = now;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(IAuditableEntity.UpdatedAt)).CurrentValue = now;
                }
            }

            foreach (var entry in context.ChangeTracker.Entries())
            {
                foreach (var prop in entry.Properties)
                {
                    if (prop.CurrentValue is DateTime dt && dt.Kind != DateTimeKind.Unspecified)
                    {
                        prop.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
                    }
                }
            }
        }
    }
}
