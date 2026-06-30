using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Directo.Wari.TarifaEngine.Application.Common.Options;
using Directo.Wari.TarifaEngine.Application.Features.DescargarMaestro.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.DescargarMaestro.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class DescargarMaestroRepository : SqlServerRepositoryBase, IDescargarMaestroRepository
    {
        private readonly ICacheService _cache;
        private readonly ConfigurationRedisOptions _redisOptions;

        public DescargarMaestroRepository(IConfiguration configuration, ICacheService cache, IOptions<ConfigurationRedisOptions> redisOptions) : base(configuration)
        {
            _cache = cache;
            _redisOptions = redisOptions.Value;
        }

        public async Task<List<GenericExtensionResponseDto>> RecuperaTipoServicioCobertura(int IdEmpresa, int I007_Dispositivo, CancellationToken cancellationToken)
        {
            //TODO: Consulta configurarda para CACHE RecuperaTipoServicioCobertura(int IdEmpresa, int I007_Dispositivo, CancellationToken cancellationToken)
            var cacheKey = $"MD_TARIFAENGINE_TipoServicioCobertura_{IdEmpresa}_{I007_Dispositivo}";
            var lista = await _cache.GetAsync<List<GenericExtensionResponseDto>>(cacheKey, cancellationToken);

            if (lista == null)
            {
                lista = new List<GenericExtensionResponseDto>();
                await using var connection = CreateConnection();
                await using var command = CreateStoredProcedure(connection, SPName.DescargarMaestro.EX5_GenericaDispositivoTipoServicioCobertura);

                SqlParameterHelper.AddParameter(command, "@IdEmpresa", SqlDbType.Int, IdEmpresa);
                SqlParameterHelper.AddParameter(command, "@I007_Dispositivo", SqlDbType.Int, I007_Dispositivo);

                await connection.OpenAsync(cancellationToken);

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    lista.Add(Map(reader));
                }

                await _cache.SetAsync(
                    cacheKey,
                    lista,
                    TimeSpan.FromMinutes(_redisOptions.ExpirationCacheMinutes),
                    cancellationToken);
            }

            return lista;
        }

        public async Task<List<GenericExtensionResponseDto>> RecuperaTipoPagoClienteCobertura(int IdEmpresa, int I007_Dispositivo, int idCliente, CancellationToken cancellationToken)
        {
            //TODO: Consulta configurarda para CACHE RecuperaTipoPagoClienteCobertura(int IdEmpresa, int I007_Dispositivo, int idCliente, CancellationToken cancellationToken)
            var cacheKey = $"MD_TARIFAENGINE_TipoPagoClienteCobertura_{IdEmpresa}_{I007_Dispositivo}_{idCliente}";
            var lista = await _cache.GetAsync<List<GenericExtensionResponseDto>>(cacheKey, cancellationToken);

            if (lista == null)
            {
                lista = new List<GenericExtensionResponseDto>();
                await using var connection = CreateConnection();
                await using var command = CreateStoredProcedure(connection, SPName.DescargarMaestro.EX5_GenericaDispositivoTipoPagoClienteCobertura);

                SqlParameterHelper.AddParameter(command, "@IdEmpresa", SqlDbType.Int, IdEmpresa);
                SqlParameterHelper.AddParameter(command, "@I007_Dispositivo", SqlDbType.Int, I007_Dispositivo);
                SqlParameterHelper.AddParameter(command, "@IdCliente", SqlDbType.Int, idCliente);

                await connection.OpenAsync(cancellationToken);

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    lista.Add(Map(reader));
                }

                await _cache.SetAsync(
                    cacheKey,
                    lista,
                    TimeSpan.FromMinutes(_redisOptions.ExpirationCacheMinutes),
                    cancellationToken);
            }

            return lista;
        }

        private GenericExtensionResponseDto Map(SqlDataReader reader)
        {
            return new GenericExtensionResponseDto
            {
                CODI_ORDEN = reader.GetInt32("CODI_ORDEN"),
                CODL_CAMPO = reader.GetNullableString("CODL_CAMPO"),
                ISOCountryCode = reader.GetNullableString("ISOCountryCode")
            };
        }
    }
}
