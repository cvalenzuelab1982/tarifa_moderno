using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Directo.Wari.TarifaEngine.Application.Common.Models;
using Directo.Wari.TarifaEngine.Application.Common.Options;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Interfaces;
using Directo.Wari.TarifaEngine.Domain.Enums;
using Directo.Wari.TarifaEngine.Domain.Exceptions;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class TarifaRepository : SqlServerRepositoryBase, ITarifaRepository
    {
        private readonly ICacheService _cache;
        private readonly ConfigurationRedisOptions _redisOptions;

        public TarifaRepository(IConfiguration configuration, ICacheService cache, IOptions<ConfigurationRedisOptions> redisOptions) : base(configuration)
        {
            _cache = cache;
            _redisOptions = redisOptions.Value;
        }

        public async Task<BeanCoberturaResponseDto?> RecuperarCobertura(double latitud,double longitud,CancellationToken cancellationToken)
        {
            //TODO: Consulta configurarda para CACHE RecuperarCobertura(double latitud,double longitud,CancellationToken cancellationToken)
            var cacheKey = GeoCacheKeyHelper.Build("MD_TARIFAENGINE_RecuperarCobertura",latitud,longitud);

            var cached = await _cache.GetAsync<BeanCoberturaResponseDto?>(cacheKey, cancellationToken);

            if (cached != null)
                return cached;

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection,SPName.Tarifa.X1_VERIFICAR_COBERTURA);

            SqlParameterHelper.AddParameter(command, "@Latitud", SqlDbType.Float, latitud);
            SqlParameterHelper.AddParameter(command, "@Longitud", SqlDbType.Float, longitud);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync())
            {
                await _cache.SetAsync<BeanCoberturaResponseDto?>(
                    cacheKey,
                    null,
                    TimeSpan.FromMinutes(_redisOptions.ExpirationCacheMinutes),
                    cancellationToken);

                return null;
            }

            var result = Map(reader);

            await _cache.SetAsync(
                cacheKey,
                result,
                TimeSpan.FromMinutes(_redisOptions.ExpirationCacheMinutes),
                cancellationToken);

            return result;
        }

        public async Task<BeanCoberturaResponseDto?> RecuperarCoberturaOrigen(double latitud, double longitud, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.X2_VERIFICAR_COBERTURA);

            SqlParameterHelper.AddParameter(command, "@Latitud", SqlDbType.Float, latitud);
            SqlParameterHelper.AddParameter(command, "@Longitud", SqlDbType.Float, longitud);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return Map(reader);
        }

        public async Task<bool> ValidarZona(int idZonaOrigen, int idZonaDestino, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.VALIDAR_ZONAS);

            SqlParameterHelper.AddParameter(command, "@ZonaOrigen", SqlDbType.Int, idZonaOrigen);
            SqlParameterHelper.AddParameter(command, "@ZonaDestino", SqlDbType.Int, idZonaDestino);

            await connection.OpenAsync(cancellationToken);

            var result = await command.ExecuteScalarAsync(cancellationToken);

            return result != null && (bool)result;
        }

        public async Task<(bool Existe, int IdZona)> VerificarPuntoEnZonaIdZona(double latitud,double longitud,CancellationToken cancellationToken)
        {
            //TODO: Consulta configurarda para CACHE VerificarPuntoEnZonaIdZona(double latitud,double longitud,CancellationToken cancellationToken)
            var cacheKey = GeoCacheKeyHelper.Build("MD_TARIFAENGINE_VerificarPuntoEnZonaIdZona", latitud, longitud);
            var cacheItem = await _cache.GetAsync<CacheItem<int>>(cacheKey, cancellationToken);

            if (cacheItem != null)
            {
                return (cacheItem.HasValue && cacheItem.Value > 0, cacheItem.Value);
            }

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.X1_VERIFICAR_PUNTO_ZONA);

            SqlParameterHelper.AddParameter(command, "@Latitud", SqlDbType.Float, latitud);
            SqlParameterHelper.AddParameter(command, "@Longitud", SqlDbType.Float, longitud);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            int idZona = 0;
            bool existe = false;

            if (await reader.ReadAsync(cancellationToken))
            {
                idZona = reader.GetInt32(reader.GetOrdinal("IdZona"));
                existe = idZona > 0;
            }

            await _cache.SetAsync(
                cacheKey,
                new CacheItem<int>
                {
                    HasValue = true,
                    Value = idZona
                },
                TimeSpan.FromMinutes(_redisOptions.ExpirationCacheMinutes),
                cancellationToken);

            return (existe, idZona);
        }

        public async Task<decimal> ObtenerTarifaCustom(int idZonaOrigen, int idZonaDestino, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.X1_OBTENER_TARIFA_ZONA_PARTICULAR);

            SqlParameterHelper.AddParameter(command, "@idZonaOrigen", SqlDbType.Int, idZonaOrigen);
            SqlParameterHelper.AddParameter(command, "@idZonaDestino", SqlDbType.Int, idZonaDestino);

            await connection.OpenAsync(cancellationToken);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            if (result == null || result == DBNull.Value)
                return 0;

            return Convert.ToDecimal(result);
        }

        public async Task<Dictionary<string, string>> ObtenerTarifaFormulaEmpresa(int idEmpresa, int tipoServicio, CancellationToken cancellationToken)
        {
            //TODO: Consulta configurarda para CACHE ObtenerTarifaFormulaEmpresa(int idEmpresa, int tipoServicio, CancellationToken cancellationToken)
            var cacheKey = $"MD_TARIFAENGINE_ObtenerTarifaFormulaEmpresa_{idEmpresa}_{tipoServicio}";
            var parametros = await _cache.GetAsync<Dictionary<string, string>>(cacheKey, cancellationToken);

            if (parametros != null) return parametros;

            parametros = new Dictionary<string, string>();

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.X1_WS_OBTENER_PARAMETROS_FORMULA);

            SqlParameterHelper.AddParameter(command, "@IdEmpresa", SqlDbType.Int, idEmpresa);
            SqlParameterHelper.AddParameter(command, "@TipoServicio", SqlDbType.Int, tipoServicio);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var parameter = reader["Parameter"]?.ToString();
                var value = reader["Value"]?.ToString();

                if (!string.IsNullOrEmpty(parameter))
                    parametros[parameter] = value ?? string.Empty;
            }

            if (parametros.Count == 0)
                throw new BusinessRuleException("Tarifa no configurada.");

            await _cache.SetAsync(
                cacheKey,
                parametros,
                TimeSpan.FromMinutes(_redisOptions.ExpirationCacheMinutes),
                cancellationToken);

            return parametros;
        }

        private BeanCoberturaResponseDto Map(SqlDataReader reader)
        {
            return new BeanCoberturaResponseDto
            {
                ISOCountryCode = reader.GetString("ISOCountryCode"),
                descripcion = reader.GetNullableString("Descripcion"),
                activo = reader.GetBoolean("Activo"),
                codigoPais = reader.GetNullableString("CodigoPais")
            };
        }

        public async Task<bool> IsZonaAeropuerto(double latitud, double longitud, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.EX1_WS_VERIFICAR_PUNTO_ZONA_AEROPUERTO);

            SqlParameterHelper.AddParameter(command, "@Latitud", SqlDbType.Float, latitud);
            SqlParameterHelper.AddParameter(command, "@Longitud", SqlDbType.Float, longitud);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            return await reader.ReadAsync(cancellationToken);
        }

        public async Task<decimal> ObtenerConstanteZonaV2(double latitud, double longitud, int idEmpresa, CancellationToken cancellationToken)
        {
            //TODO: Consulta configurarda para CACHE ObtenerConstanteZonaV2(double latitud, double longitud, int idEmpresa, CancellationToken cancellationToken)
            var cacheKey = GeoCacheKeyHelper.Build("MD_TARIFAENGINE_ObtenerConstanteZonaV2", latitud, longitud, idEmpresa);

            var cached = await _cache.GetAsync<decimal?>(cacheKey, cancellationToken);

            if (cached.HasValue) return cached.Value;

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.X1_WS_OBTENER_CONSTANTE_ZONA_EMPRESA);

            SqlParameterHelper.AddParameter(command, "@Latitud", SqlDbType.Float, latitud);
            SqlParameterHelper.AddParameter(command, "@Longitud", SqlDbType.Float, longitud);
            SqlParameterHelper.AddParameter(command, "@IdEmpresa", SqlDbType.Float, idEmpresa);

            await connection.OpenAsync(cancellationToken);
            var scalar = await command.ExecuteScalarAsync(cancellationToken);
            var result = (scalar == null || scalar == DBNull.Value) ? 0m : Convert.ToDecimal(scalar);

            await _cache.SetAsync(
                cacheKey,
                result,
                TimeSpan.FromMinutes(_redisOptions.ExpirationCacheMinutes),
                cancellationToken);

            return result;
        }

        public async Task<bool> VerificarPuntoEnZonaTipoZona(double latitud, double longitud, TipoZona tipoZona, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.X1_VERIFICAR_MULTIPLE_PUNTO_ZONA);

            SqlParameterHelper.AddParameter(command, "@Latitud", SqlDbType.Float, latitud);
            SqlParameterHelper.AddParameter(command, "@Longitud", SqlDbType.Float, longitud);
            SqlParameterHelper.AddParameter(command, "@idZona", SqlDbType.Int, 0);
            SqlParameterHelper.AddParameter(command, "@tipoZona", SqlDbType.Int, (int)tipoZona);
            SqlParameterHelper.AddParameter(command, "@accion", SqlDbType.Int, 3);

            await connection.OpenAsync(cancellationToken);
            var result = await command.ExecuteScalarAsync(cancellationToken);

            if (result == null || result == DBNull.Value)
                return false;

            return Convert.ToInt32(result) > 0;
        }

        public async Task<Dictionary<string, string>> ObtenerTarifaFormulaEmpresaCobertura(int idEmpresa, int tipoServicio, string cobertura, CancellationToken cancellationToken)
        {
            //TODO: Consulta configurarda para CACHE ObtenerTarifaFormulaEmpresaCobertura(int idEmpresa, int tipoServicio, string cobertura, CancellationToken cancellationToken)
            var cacheKey = $"MD_TARIFAENGINE_ObtenerTarifaFormulaEmpresaCobertura_{idEmpresa}_{tipoServicio}_{cobertura}";
            var parametros = await _cache.GetAsync<Dictionary<string, string>>(cacheKey, cancellationToken);

            if (parametros != null) return parametros;

            parametros = new Dictionary<string, string>();

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.X1_WS_OBTENER_PARAMETROS_FORMULA_ISOCOUNTRY);

            SqlParameterHelper.AddParameter(command, "@IdEmpresa", SqlDbType.Int, idEmpresa);
            SqlParameterHelper.AddParameter(command, "@TipoServicio", SqlDbType.Int, tipoServicio);
            SqlParameterHelper.AddParameter(command, "@Cobertura", SqlDbType.VarChar, cobertura);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var parameter = reader["Parameter"]?.ToString();
                var value = reader["Value"]?.ToString();

                if (!string.IsNullOrEmpty(parameter))
                    parametros[parameter] = value ?? string.Empty;
            }

            if (parametros.Count == 0)
                throw new BusinessRuleException("Tarifa no configurada.");

            await _cache.SetAsync(
                cacheKey,
                parametros,
                TimeSpan.FromMinutes(_redisOptions.ExpirationCacheMinutes),
                cancellationToken);

            return parametros;
        }

        public async Task<decimal> ObtenerConstanteZona(double latitud, double longitud, CancellationToken cancellationToken)
        {
            //TODO: Consulta configurarda para CACHE ObtenerConstanteZona(double latitud, double longitud, CancellationToken cancellationToken)
            var cacheKey = GeoCacheKeyHelper.Build("MD_TARIFAENGINE_ObtenerConstanteZona_", latitud, longitud);

            var cached = await _cache.GetAsync<decimal?>(cacheKey, cancellationToken);

            if (cached.HasValue) return cached.Value;

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.X1_WS_OBTENER_CONSTANTE_ZONA);

            SqlParameterHelper.AddParameter(command, "@Latitud", SqlDbType.Float, latitud);
            SqlParameterHelper.AddParameter(command, "@Longitud", SqlDbType.Float, longitud);

            await connection.OpenAsync(cancellationToken);
            var scalar = await command.ExecuteScalarAsync(cancellationToken);
            var result = (scalar == null || scalar == DBNull.Value) ? 0m : Convert.ToDecimal(scalar);


            await _cache.SetAsync(
                cacheKey,
                result,
                TimeSpan.FromMinutes(_redisOptions.ExpirationCacheMinutes),
                cancellationToken);

            return result;
        }

        public async Task<bool> IsZonaDirecto(double latitud, double longitud, int idZona, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.X1_VERIFICAR_MULTIPLE_PUNTO_ZONA);

            SqlParameterHelper.AddParameter(command, "@Latitud", SqlDbType.Float, latitud);
            SqlParameterHelper.AddParameter(command, "@Longitud", SqlDbType.Float, longitud);
            SqlParameterHelper.AddParameter(command, "@idZona", SqlDbType.Int, idZona);
            SqlParameterHelper.AddParameter(command, "@tipoZona", SqlDbType.Int, 0);
            SqlParameterHelper.AddParameter(command, "@accion", SqlDbType.Int, 2);

            await connection.OpenAsync(cancellationToken);
            var result = await command.ExecuteScalarAsync(cancellationToken);

            if (result == null || result == DBNull.Value)
                return false;

            return Convert.ToInt32(result) > 0;
        }

        public async Task<decimal> ObtenerDescuentoSedeServicio(DetBeanServicioRequestSPDto inputBeanServ, oldBeanTarifaRequestDto beanTarifa, CancellationToken cancellationToken)
        {
            decimal descuentoKm = 0;

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.EX1_OBTENER_DESCUENTO_MONTO);

            SqlParameterHelper.AddParameter(command, "@idEmpresa", SqlDbType.Int, inputBeanServ.IdEmpresa);
            SqlParameterHelper.AddParameter(command, "@tipoServicio", SqlDbType.Int, inputBeanServ.TipoServicio);

            var formaCalculo = string.IsNullOrEmpty(beanTarifa.FormaCalculo)
                ? 0
                : Convert.ToInt32(beanTarifa.FormaCalculo);

            SqlParameterHelper.AddParameter(command, "@formaCalculo", SqlDbType.Int, formaCalculo);
            SqlParameterHelper.AddParameter(command, "@kilometros", SqlDbType.Decimal, beanTarifa.Kilometros);
            SqlParameterHelper.AddParameter(command, "@origenLatitud", SqlDbType.Float, inputBeanServ.OrigenLatitud);
            SqlParameterHelper.AddParameter(command, "@origenLongitud", SqlDbType.Float, inputBeanServ.OrigenLongitud);
            SqlParameterHelper.AddParameter(command, "@destinoLatitud", SqlDbType.Float, inputBeanServ.DestinoLatitud);
            SqlParameterHelper.AddParameter(command, "@destinoLongitud", SqlDbType.Float, inputBeanServ.DestinoLongitud);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                descuentoKm = reader.HasColumn("Descuento")
                    ? reader.GetNullableDecimal("Descuento") ?? 0
                    : 0;
            }

            return descuentoKm;
        }

        public async Task<Dictionary<string, decimal>?> ObtenerMontoIncrementoEmpresa(int idEmpresa, CancellationToken cancellationToken)
        {
            Dictionary<string, decimal>? result = null;

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.OBTENER_MONTO_INCREMENTO_EMPRESA_X1);

            SqlParameterHelper.AddParameter(command, "@idEmpresa", SqlDbType.Int, idEmpresa);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var totalIncremento = reader.GetNullableDecimal("TOTAL_INCREMENTO") ?? 0;
                var totalIncrementoEmpresa = reader.GetNullableDecimal("TOTAL_INCREMENTO_EMPRESA") ?? 0;

                result = new Dictionary<string, decimal>
                {
                    ["TOTAL_INCREMENTO"] = totalIncremento,
                    ["TOTAL_INCREMENTO_EMPRESA"] = totalIncrementoEmpresa
                };
            }

            return result;
        }

        public async Task<RecuperaByIdFormaCalculoResponseDto> RecuperaByIdFormaCalculo(int Empresa, int I011_TipoServicio, int I008_TipoPago, int Origen, int Destino, int idCliente, int cantPasajeros = 0, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.EX3_WS_TarifaByFormaCalculo);

            SqlParameterHelper.AddParameter(command, "@Empresa", SqlDbType.Int, Empresa);
            SqlParameterHelper.AddParameter(command, "@I011_TipoServicio", SqlDbType.Int, I011_TipoServicio);
            SqlParameterHelper.AddParameter(command, "@I008_TipoPago", SqlDbType.Int, I008_TipoPago);
            SqlParameterHelper.AddParameter(command, "@Origen", SqlDbType.Int, Origen);
            SqlParameterHelper.AddParameter(command, "@Destino", SqlDbType.Int, Destino);
            SqlParameterHelper.AddParameter(command, "@Cliente", SqlDbType.Int, idCliente);
            SqlParameterHelper.AddParameter(command, "@CantPax", SqlDbType.Int, cantPasajeros);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                return MapRecuperaByIdFormaCalculo(reader);
            }

            return new RecuperaByIdFormaCalculoResponseDto();
        }

        public async Task<RecuperaByIdFormaCalculoResponseDto> RecuperaByIdFormaCalculoLatam(int Empresa, int idCliente, int I011_TipoServicio, int I008_TipoPago, int Origen, int Destino, int cantPasajeros = 0, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.EX3_WS_TarifaByFormaCalculo);

            SqlParameterHelper.AddParameter(command, "@Empresa", SqlDbType.Int, Empresa);
            SqlParameterHelper.AddParameter(command, "@I011_TipoServicio", SqlDbType.Int, I011_TipoServicio);
            SqlParameterHelper.AddParameter(command, "@I008_TipoPago", SqlDbType.Int, I008_TipoPago);
            SqlParameterHelper.AddParameter(command, "@Origen", SqlDbType.Int, Origen);
            SqlParameterHelper.AddParameter(command, "@Destino", SqlDbType.Int, Destino);
            SqlParameterHelper.AddParameter(command, "@Cliente", SqlDbType.Int, idCliente);
            SqlParameterHelper.AddParameter(command, "@CantPax", SqlDbType.Int, cantPasajeros);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                return MapRecuperaByIdFormaCalculo(reader);
            }

            return new RecuperaByIdFormaCalculoResponseDto();
        }

        private RecuperaByIdFormaCalculoResponseDto MapRecuperaByIdFormaCalculo(SqlDataReader reader)
        {
            return new RecuperaByIdFormaCalculoResponseDto
            {
                Monto = reader.HasColumn("Monto")
                    ? reader.GetNullableDecimal("Monto") ?? 0
                    : 0,

                TotalServicioDolares = reader.HasColumn("totalServicioDolares")
                    ? reader.GetNullableDecimal("totalServicioDolares") ?? 0
                    : 0
            };
        }

        public async Task<RecuperaByIdResponseDto?> RecuperaById(int Empresa, int Cliente, int I011_TipoServicio, int I008_TipoPago, int Origen, int Destino, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Tarifa.EX1_TarifaByTipoServicio);

            SqlParameterHelper.AddParameter(command, "@Empresa", SqlDbType.Int, Empresa);
            SqlParameterHelper.AddParameter(command, "@Cliente", SqlDbType.Int, Cliente);
            SqlParameterHelper.AddParameter(command, "@I011_TipoServicio", SqlDbType.Int, I011_TipoServicio);
            SqlParameterHelper.AddParameter(command, "@I008_TipoPago", SqlDbType.Int, I008_TipoPago);
            SqlParameterHelper.AddParameter(command, "@Origen", SqlDbType.Int, Origen);
            SqlParameterHelper.AddParameter(command, "@Destino", SqlDbType.Int, Destino);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                return new RecuperaByIdResponseDto
                {
                    Monto = reader.GetDecimal(0),
                    Abono = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1),
                    PagoAdelantado = !reader.IsDBNull(2) && reader.GetBoolean(2)
                };
            }

            return null;
        }
    }
}
