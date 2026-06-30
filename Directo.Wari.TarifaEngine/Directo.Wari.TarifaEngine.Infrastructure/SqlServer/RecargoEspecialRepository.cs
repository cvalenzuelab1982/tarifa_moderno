using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Directo.Wari.TarifaEngine.Application.Common.Models;
using Directo.Wari.TarifaEngine.Application.Common.Options;
using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class RecargoEspecialRepository : SqlServerRepositoryBase, IRecargoEspecialRepository
    {
        private readonly ICacheService _cache;
        private readonly ConfigurationRedisOptions _redisOptions;

        public RecargoEspecialRepository(IConfiguration configuration, ICacheService cache, IOptions<ConfigurationRedisOptions> redisOptions) : base(configuration)
        {
            _cache = cache;
            _redisOptions = redisOptions.Value;
        }

        public async Task<BeanRecargoEspecialResponseDto?> GetRecargoEspecialAeropuerto(int IdEmpresa, int IdTipoPago, int IdTipoServicio, DateTime FechaServicio, int OrigenDestino, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.RecargoEspecial.EX2_VALIDAR_HORA_PUNTA_AEROPUERTO);

            SqlParameterHelper.AddParameter(command, "@IdEmpresa", SqlDbType.Int, IdEmpresa);
            SqlParameterHelper.AddParameter(command, "@IdTipoServicio", SqlDbType.Int, IdTipoServicio);
            SqlParameterHelper.AddParameter(command, "@IdTipoPago", SqlDbType.Int, IdTipoPago);
            SqlParameterHelper.AddParameter(command, "@FechaServicio", SqlDbType.DateTime, FechaServicio);
            SqlParameterHelper.AddParameter(command, "@OrigenDestino", SqlDbType.Int, OrigenDestino);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return Map_GetRecargoEspecialAeropuerto(reader);
        }

        public async Task<BeanRecargoEspecialResponseDto?> GetRecargoEspecialTipoServicioTipoPago(int IdEmpresa, int IdTipoPago, int IdTipoServicio, DateTime FechaServicio, CancellationToken cancellationToken)
        {
            //TODO: Consulta configurarda para CACHE GetRecargoEspecialTipoServicioTipoPago(int IdEmpresa, int IdTipoPago, int IdTipoServicio, DateTime FechaServicio, CancellationToken cancellationToken)
            var cacheKey = $"MD_TARIFAENGINE_GetRecargoEspecialTipoServicioTipoPago_{IdEmpresa}_{IdTipoPago}_{IdTipoServicio}_{FechaServicio:yyyyMMddHHmm}";
            var cacheItem = await _cache.GetAsync<CacheItem<BeanRecargoEspecialResponseDto?>>(cacheKey, cancellationToken);

            if (cacheItem != null)
            {
                return cacheItem.HasValue ? cacheItem.Value : null;
            }

            BeanRecargoEspecialResponseDto? result = null;

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.RecargoEspecial.EX2_OBTENER_RECARGO_ESPECIAL_DIRECTO);

            SqlParameterHelper.AddParameter(command, "@IdEmpresa", SqlDbType.Int, IdEmpresa);
            SqlParameterHelper.AddParameter(command, "@IdTipoPago", SqlDbType.Int, IdTipoPago);
            SqlParameterHelper.AddParameter(command, "@IdTipoServicio", SqlDbType.Int, IdTipoServicio);
            SqlParameterHelper.AddParameter(command, "@FechaServicio", SqlDbType.DateTime, FechaServicio);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                result = Map_GetRecargoEspecialTipoServicioTipoPago(reader);
            }

            await _cache.SetAsync(
                cacheKey, 
                new CacheItem<BeanRecargoEspecialResponseDto> { HasValue = result != null, Value = result},
                TimeSpan.FromMinutes(_redisOptions.ExpirationCacheMinutes),
                cancellationToken);
            return result;
        }

        private BeanRecargoEspecialResponseDto Map_GetRecargoEspecialAeropuerto(SqlDataReader reader)
        {
            return new BeanRecargoEspecialResponseDto
            {
                idRecargoEspecial = reader.GetNullableInt("idCargoAeropuerto"),
                idEmpresa = reader.GetNullableInt("idEmpresa"),
                ValorRecargo = reader.GetDecimal("Incremento"),
                Activo = reader.GetNullableBool("Activo"),
                tipoRecargo = reader.GetInt32("tipoCalculo"),
                tipoRedondeo = reader.GetNullableString("TipoRedondeo"),
                cantidadDecimal = reader.GetNullableInt("CantidadDecimal") ?? 2
            };
        }

        private BeanRecargoEspecialResponseDto Map_GetRecargoEspecialTipoServicioTipoPago(SqlDataReader reader)
        {
            return new BeanRecargoEspecialResponseDto
            {
                idRecargoEspecial = reader.GetNullableInt("idRecargoEspecial"),
                idTipoPago = reader.GetNullableInt("idEmpresa"),
                idEmpresa = reader.GetNullableInt("idEmpresa"),
                ValorRecargo = reader.GetDecimal("valorRecargo"),
                Activo = reader.GetNullableBool("Activo"),
                tipoRecargo = reader.GetInt32("tipoRecargo"),
                tipoRedondeo = reader.GetNullableString("tipoRedondeo"),
                cantidadDecimal = reader.GetNullableInt("CantidadDecimal") ?? 2
            };
        }

    }
}
