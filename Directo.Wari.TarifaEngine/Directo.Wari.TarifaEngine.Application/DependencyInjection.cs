using Directo.Wari.TarifaEngine.Application.Common.Behaviors;
using Directo.Wari.TarifaEngine.Application.Common.Options;
using Directo.Wari.TarifaEngine.Application.Features.Promociones.Services;
using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Services;
using Directo.Wari.TarifaEngine.Application.Features.RecargoReserva.Services;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Services.TarifaCalculatorService;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Directo.Wari.TarifaEngine.Application
{
    /// <summary>
    /// Extensión para registrar los servicios de la capa de Application en el contenedor DI.
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
            });

            // Pipeline Behaviors (orden importa)
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

            // FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // Mapster
            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            typeAdapterConfig.Scan(assembly);
            services.AddSingleton(typeAdapterConfig);
            services.AddScoped<IMapper, ServiceMapper>();

            // Repositories de servicios de negocio
            services.AddScoped<ITarifaCalculatorService, TarifaCalculatorService>();
            services.AddScoped<IRecargoEspecialService, RecargoEspecialService>();
            services.AddScoped<IRecargoReservaService, RecargoReservaService>();
            services.AddScoped<IPromocionesService, PromocionesService>();

            // Options Pattern
            services.Configure<ConfiguracionGenericasOptions>(configuration.GetSection("ConfiguracionMoneda"));
            services.Configure<ConfigurationRedisOptions>(configuration.GetSection("Redis"));
            services.Configure<HttpExternasOptions>(configuration.GetSection("HttpExternas"));

            return services;
        }
    }
}
