using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Directo.Wari.TarifaEngine.Application.Common.Models;
using Directo.Wari.TarifaEngine.Application.Common.Options;
using Directo.Wari.TarifaEngine.Application.Features.Servicio.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Zona.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class ServicioRepository : SqlServerRepositoryBase, IServicioRepository
    {
        private readonly IZonaRepository _zonaRepository;
        private readonly ITarifaRepository _tarifaRepository;
        private readonly ILogger<ServicioRepository> _logger;
        private readonly ICacheService _cache;
        private readonly ConfigurationRedisOptions _redisOptions;

        public ServicioRepository(IOptions<ConfigurationRedisOptions> redisOptions, ILogger<ServicioRepository> logger, IConfiguration configuration, IZonaRepository zonaRepository, ITarifaRepository tarifaRepository, ICacheService cache) : base(configuration)
        {
            _zonaRepository = zonaRepository;
            _tarifaRepository = tarifaRepository;
            _logger = logger;
            _cache = cache;
            _redisOptions = redisOptions.Value;

        }

        public async Task<bool> TodosPrimerDestinoEmpresa(int idEmpresa, CancellationToken cancellationToken)
        {
            var cacheKey = $"MD_TARIFAENGINE_TodosPrimerDestinoEmpresa_{idEmpresa}";

            var cacheItem = await _cache.GetAsync<CacheItem<bool>>(cacheKey, cancellationToken);

            if (cacheItem != null)
            {
                return cacheItem.HasValue && cacheItem.Value;
            }

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Servicio.X1_WS_TodosPrimerDestino);

            SqlParameterHelper.AddParameter(command, "@idEmpresa", SqlDbType.Int, idEmpresa);

            await connection.OpenAsync(cancellationToken);

            var result = await command.ExecuteScalarAsync(cancellationToken);

            bool value = false;

            if (result != null && result != DBNull.Value)
            {
                value = Convert.ToBoolean(result);
            }

            await _cache.SetAsync(
                cacheKey,
                new CacheItem<bool>
                {
                    HasValue = result != null,
                    Value = value
                },
                TimeSpan.FromMinutes(_redisOptions.ExpirationCacheMinutes),
                cancellationToken);

            return value;
        }

        public async Task<string> ObteneTipoFormaCalculoEmpresa(int idTipoServicio, int dispositivo, int idEmpresa, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Servicio.X2_WS_FORMA_CALCULO_TIPO_SERVICIO);

            SqlParameterHelper.AddParameter(command, "@TipoNegocio", SqlDbType.Int, idTipoServicio);
            SqlParameterHelper.AddParameter(command, "@IdDispositivo", SqlDbType.Int, dispositivo);
            SqlParameterHelper.AddParameter(command, "@IdEmpresa", SqlDbType.Int, idEmpresa);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                var value = reader["FORMA_CALCULO_USAR"];
                return value == DBNull.Value ? "0" : value.ToString()!;
            }

            return "0";
        }

        public async Task<int> ObtenerTiempoPorZona(int idZona, DateTime horaActual, int idEmpresa, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Servicio.OBTENER_TIEMPO_POR_POR_ZONA);

            SqlParameterHelper.AddParameter(command, "@idZona", SqlDbType.Int, idZona);
            SqlParameterHelper.AddParameter(command, "@HoraActual", SqlDbType.DateTime, horaActual);
            SqlParameterHelper.AddParameter(command, "@idEmpresa", SqlDbType.Int, idEmpresa);

            await connection.OpenAsync(cancellationToken);

            var result = await command.ExecuteScalarAsync(cancellationToken);

            return result is int value ? value : Convert.ToInt32(result ?? 0);
        }


        public async Task<int> TiempoReservaAlo(int idZona, DateTime horaActual, int idEmpresa)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Servicio.OBTENER_TIEMPO_POR_ZONA_ALO_X2);

            SqlParameterHelper.AddParameter(command, "@idZona", SqlDbType.Int, idZona);
            SqlParameterHelper.AddParameter(command, "@HoraActual", SqlDbType.DateTime, horaActual);
            SqlParameterHelper.AddParameter(command, "@idEmpresa", SqlDbType.Int, idEmpresa);

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            return result != null ? Convert.ToInt32(result) : 0;
        }

    }
}
