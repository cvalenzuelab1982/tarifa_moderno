using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Directo.Wari.TarifaEngine.Application.Common.Options;
using Directo.Wari.TarifaEngine.Application.Features.Parametros.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Parametros.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class ParametrosRepository : SqlServerRepositoryBase, IParametrosRepository
    {
        private readonly ICacheService _cache;
        private readonly ConfigurationRedisOptions _redisOptions;

        public ParametrosRepository(IConfiguration configuration, ICacheService cache, IOptions<ConfigurationRedisOptions> redisOptions) : base(configuration)
        {
            _cache = cache;
            _redisOptions = redisOptions.Value;
        }

        public async Task<string> GetParameterValue(string nombreParametro, CancellationToken cancellationToken)
        {
            //TODO: Consulta configurarda para CACHE GetParameterValue(string nombreParametro, CancellationToken cancellationToken)
            var cacheKey = "MD_TARIFAENGINE_ParameterValue";
            var lista = await _cache.GetAsync<List<ParametroResponseDto>>(cacheKey, cancellationToken);

            if (lista == null)
            {
                lista = new List<ParametroResponseDto>();
                await using var connection = CreateConnection();
                await using var command = CreateStoredProcedure(connection, SPName.Parametros.PARAMETRO_LISTAR_TODO);

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

            var parametro = lista.FirstOrDefault(x => x.Nombre_Parametro == nombreParametro);

            if (parametro == null)
                return string.Empty;

            return parametro.valor_parametro;
        }

        private ParametroResponseDto Map(SqlDataReader reader)
        {
            return new ParametroResponseDto
            {
                Nombre_Parametro = reader.GetString("Nombre_Parametro"),
                valor_parametro = reader.GetString("valor_parametro")
            };
        }
    }
}
