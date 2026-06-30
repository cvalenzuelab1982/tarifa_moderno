using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Cliente.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Compania.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Conductor.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.DescargarMaestro.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.HoraPunta.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.HttpApi.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Parametros.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Peaje.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Plaza.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Promociones.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.RecargoReserva.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Servicio.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Zona.Interfaces;
using Directo.Wari.TarifaEngine.Domain.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Caching;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Interceptors;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Repositories;
using Directo.Wari.TarifaEngine.Infrastructure.Services;
using Directo.Wari.TarifaEngine.Infrastructure.Services.HttpApi;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Directo.Wari.TarifaEngine.Infrastructure
{
    /// <summary>
    /// Extensión para registrar los servicios de infraestructura en el contenedor DI.
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Interceptors
            services.AddSingleton<AuditableEntityInterceptor>();

            // PostgreSQL + EF Core
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

                options.UseNpgsql(
                        configuration.GetConnectionString("DefaultConnection"),
                        npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                            npgsqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 3,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorCodesToAdd: null);
                        })
                    .AddInterceptors(interceptor);
            });



            // Registrar DbContext interfaces
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Repositories consulta a solo SP
            services.AddScoped<IParametrosRepository, ParametrosRepository>();
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<ITarifaRepository, TarifaRepository>();
            services.AddScoped<IZonaRepository, ZonaRepository>();
            services.AddScoped<IDescargarMaestroRepository, DescargarMaestroRepository>();
            services.AddScoped<IServicioRepository, ServicioRepository>();
            services.AddScoped<IRecargoEspecialRepository, RecargoEspecialRepository>();
            services.AddScoped<IRecargoReservaRepository, RecargoReservaRepository>();
            services.AddScoped<IHoraPuntaRepository, HoraPuntaRepository>();
            services.AddScoped<IPlazaRepository, PlazaRepository>();
            services.AddScoped<ICompaniaRepository, CompaniaRepository>();
            services.AddScoped<IConductorRepository, ConductorRepository>();
            services.AddScoped<IPeajeRepository, PeajeRepository>();
            services.AddScoped<IPromocionesRepository, PromocionesRepository>();



            // Redis Cache
            var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString");
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "WariDirecto_v2:";
                });
                services.AddScoped<ICacheService, RedisCacheService>();
            }

            // Services
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
            services.AddHttpClient<IHttpApiRepository, HttpApiRepository>();

            return services;
        }
    }
}
