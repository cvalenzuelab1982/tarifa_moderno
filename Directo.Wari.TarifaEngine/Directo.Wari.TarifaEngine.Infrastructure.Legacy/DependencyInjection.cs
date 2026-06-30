using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Directo.Wari.TarifaEngine.Domain.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Legacy.Configuration;
using Directo.Wari.TarifaEngine.Infrastructure.Legacy.Persistence;
using Directo.Wari.TarifaEngine.Infrastructure.Legacy.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Directo.Wari.TarifaEngine.Infrastructure.Legacy
{
    /// <summary>
    /// Extensión para registrar los servicios del sistema legacy en el contenedor DI.
    /// 
    /// Controla las fases de migración:
    /// - Fase 1: ReadSource=SqlServer, SyncToLegacy=true  → Lee de SQL Server, escribe en ambas.
    /// - Fase 2: ReadSource=PostgreSql, SyncToLegacy=true  → Lee de PostgreSQL, escribe en ambas.
    /// - Fase 3: ReadSource=PostgreSql, SyncToLegacy=false → Solo PostgreSQL (eliminar este proyecto).
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddLegacyServices(
       this IServiceCollection services,
       IConfiguration configuration)
        {
            // Bind opciones de migración
            var migrationSection = configuration.GetSection(MigrationOptions.SectionName);
            services.Configure<MigrationOptions>(migrationSection);

            var migrationOptions = migrationSection.Get<MigrationOptions>() ?? new MigrationOptions();

            // Si no hay connection string legacy, no registrar nada
            var legacyConnectionString = configuration.GetConnectionString("LegacyConnection");
            if (string.IsNullOrEmpty(legacyConnectionString))
            {
                // Si no hay conexión legacy pero se requiere sync, advertir
                if (migrationOptions.SyncToLegacy || migrationOptions.ReadFromLegacy)
                {
                    // Registrar un logger temporal para advertir en startup
                    var sp = services.BuildServiceProvider();
                    var logger = sp.GetService<ILogger<LegacyDbContext>>();
                    logger?.LogWarning(
                        "Configuración de migración requiere SQL Server (SyncToLegacy={Sync}, ReadSource={Read}) " +
                        "pero no se encontró 'ConnectionStrings:LegacyConnection'. Servicios legacy NO registrados.",
                        migrationOptions.SyncToLegacy, migrationOptions.ReadSource);
                }
                return services;
            }

            // ═══════════════════════════════════════════════════════════════
            // LEGACY DB CONTEXT (SQL Server)
            // ═══════════════════════════════════════════════════════════════
            services.AddDbContext<LegacyDbContext>(options =>
            {
                options.UseSqlServer(legacyConnectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(15),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(30);
                });
            });

            // ═══════════════════════════════════════════════════════════════
            // FASE 1: Sobreescribir IReadDbContext para leer de SQL Server
            // ═══════════════════════════════════════════════════════════════
            if (migrationOptions.ReadFromLegacy)
            {
                // Reemplazar el registro existente de IReadDbContext (que apuntaba a PostgreSQL)
                // con LegacyReadDbContext (que lee de SQL Server)
                services.AddScoped<LegacyReadDbContext>();
                services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<LegacyReadDbContext>());
            }

            // ═══════════════════════════════════════════════════════════════
            // DUAL-WRITE: Decorar IUnitOfWork para sincronizar a SQL Server
            // ═══════════════════════════════════════════════════════════════
            if (migrationOptions.SyncToLegacy)
            {
                // Decorar el IUnitOfWork existente con DualWriteUnitOfWork
                services.Decorate<IUnitOfWork>((inner, sp) =>
                {
                    var dbContext = sp.GetRequiredService<IApplicationDbContext>();
                    var logger = sp.GetRequiredService<ILogger<DualWriteUnitOfWork>>();
                    return new DualWriteUnitOfWork(inner, dbContext, logger);
                });

                // Registrar servicio de sincronización
                services.AddScoped<ILegacySyncService, LegacySyncService>();

                // Registrar el procesador de outbox como background service
                services.AddHostedService<OutboxProcessor>();
            }

            return services;
        }
    }
}
