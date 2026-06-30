using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Directo.Wari.TarifaEngine.Application.Common.Models;
using Directo.Wari.TarifaEngine.Application.Common.Options;
using Directo.Wari.TarifaEngine.Application.Features.Zona.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Zona.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class ZonaRepository : SqlServerRepositoryBase, IZonaRepository
    {
        private readonly ILogger<ZonaRepository> _logger;
        private readonly ICacheService _cache;
        private readonly ConfigurationRedisOptions _redisOptions;


        public ZonaRepository(IConfiguration configuration, ILogger<ZonaRepository> logger, ICacheService cache, IOptions<ConfigurationRedisOptions> redisOptions) : base(configuration)
        {
            _logger = logger;
            _cache = cache;
            _redisOptions = redisOptions.Value;
        }

        public async Task<int> ObtenerIdZona(double latitud, double longitud, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Zona.ZN_VERIFICAR_IDZONA);

            SqlParameterHelper.AddParameter(command, "@Latitud", SqlDbType.Float, latitud);
            SqlParameterHelper.AddParameter(command, "@Longitud", SqlDbType.Float, longitud);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                return reader.GetNullableInt("idZona") ?? 0;
            }
            else
            {
                return 0;
            }
        }

        public async Task<int> RecuperaByPosicion(int IdEmpresa, double Latitud, double Longitud, CancellationToken cancellationToken)
        {
            //TODO: Consulta configurarda para CACHE RecuperaByPosicion(int IdEmpresa, double Latitud, double Longitud, CancellationToken cancellationToken)
            var cacheKey = GeoCacheKeyHelper.Build("MD_TARIFAENGINE_RecuperaByPosicion", IdEmpresa, Latitud, Longitud);

            var cached = await _cache.GetAsync<int?>(cacheKey, cancellationToken);

            if (cached.HasValue) return cached.Value;

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Zona.EX1_ZonaByPosicionEmpresa);

            SqlParameterHelper.AddParameter(command, "@IdEmpresa", SqlDbType.Int, IdEmpresa);
            SqlParameterHelper.AddParameter(command, "@Latitud", SqlDbType.Float, Latitud);
            SqlParameterHelper.AddParameter(command, "@Longitud", SqlDbType.Float, Longitud);

            await connection.OpenAsync(cancellationToken);
            var result = await command.ExecuteScalarAsync(cancellationToken);

            int idZona = 0;

            if (result != null && result != DBNull.Value)
            {
                idZona = Convert.ToInt32(result);
            }

            await _cache.SetAsync(
                cacheKey,
                (int)idZona,
                TimeSpan.FromMinutes(_redisOptions.ExpirationCacheMinutes),
                cancellationToken);

            return idZona;
        }

        public async Task<bool> IsZonaPeligrosa(double latitud, double longitud, CancellationToken cancellationToken = default)
        {
            bool zonaPeligrosa = false;

            _logger.LogInformation("IsZonaPeligrosa IN Latitud: {Latitud}, Longitud: {Longitud}", latitud, longitud);

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Zona.ZN_VERIFICAR_ZONA_PELIGROSA);

            SqlParameterHelper.AddParameter(command, "@Latitud", SqlDbType.Float, latitud);
            SqlParameterHelper.AddParameter(command, "@Longitud", SqlDbType.Float, longitud);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                zonaPeligrosa = reader.HasColumn("ZonaPeligrosa")
                    ? reader.GetNullableBool("ZonaPeligrosa") ?? false
                    : false;
            }

            _logger.LogInformation("IsZonaPeligrosa OUT ZonaPeligrosa: {ZonaPeligrosa}", zonaPeligrosa);

            return zonaPeligrosa;
        }

        public async Task<ZonaResponseDto> RecuperaById(int Zona, CancellationToken cancellationToken)
        {
            var cacheKey = $"MD_TARIFAENGINE_RecuperaById_{Zona}";
            var cacheItem = await _cache.GetAsync<CacheItem<ZonaResponseDto>>(cacheKey, cancellationToken);

            if (cacheItem != null)
            {
                return cacheItem.HasValue ? cacheItem.Value : new ZonaResponseDto();
            }

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Zona.EX1_ZonaListar);

            SqlParameterHelper.AddParameter(command, "@IdZona", SqlDbType.Int, Zona);
            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            ZonaResponseDto result = null;
            bool hasValue = false;

            if (await reader.ReadAsync(cancellationToken))
            {
                result = Map(reader);
                hasValue = true;
            }

            await _cache.SetAsync(
                cacheKey,
                new CacheItem<ZonaResponseDto>
                {
                    HasValue = hasValue,
                    Value = result
                },
                TimeSpan.FromMinutes(_redisOptions.ExpirationCacheMinutes),
                cancellationToken);

            return result ?? new ZonaResponseDto();
        }

        private ZonaResponseDto Map(SqlDataReader reader)
        {
            var geoObj = reader.GetValue("Geocerca");
            var centObj = reader.GetValue("Centro");

            var geoDto = geoObj is DBNull
                ? new SqlGeographyResponseDto { IsNull = true }
                : new SqlGeographyResponseDto { IsNull = false };

            var centDto = centObj is DBNull
                ? new SqlGeographyResponseDto { IsNull = true }
                : new SqlGeographyResponseDto { IsNull = false };

            return new ZonaResponseDto
            {
                IdZona = reader.GetInt32("IdZona"),
                IdUbigeo = reader.GetInt32("IdUbigeo"),
                I025_TipoZona = reader.GetInt32("I025_TipoZona"),
                Descripcion = reader.GetString("Descripcion"),
                Geocerca = geoDto,
                Centro = centDto
            };
        }
    }
}
