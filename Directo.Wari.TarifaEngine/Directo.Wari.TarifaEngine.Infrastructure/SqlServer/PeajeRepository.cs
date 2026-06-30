using Directo.Wari.TarifaEngine.Application.Features.Peaje.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class PeajeRepository : SqlServerRepositoryBase, IPeajeRepository
    {
        private readonly ILogger<PeajeRepository> _logger;

        public PeajeRepository(IConfiguration configuration, ILogger<PeajeRepository> logger) : base(configuration) {
            _logger = logger;
        }

        public async Task<List<PeajeSistemaRespondeDto>> ListarPeajesPorZonas(List<int> listaIdsZonas, CancellationToken cancellationToken)
        {
            var listaPeajes = new List<PeajeSistemaRespondeDto>();
            var stringZonas = string.Join(",", listaIdsZonas);
            _logger.LogInformation("ListarPeajesPorZonas IN Zonas: {Zonas}", stringZonas);

            if (string.IsNullOrEmpty(stringZonas))
                return listaPeajes;

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Zona.NX_LISTAR_PEAJES_X_ZONAS);

            SqlParameterHelper.AddParameter(command, "@ListaZonas", SqlDbType.VarChar, stringZonas);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var obj = new PeajeSistemaRespondeDto
                {
                    IdPeaje = reader.GetNullableInt("idPeaje") ?? 0,
                    Nombrepeaje = reader.GetNullableString("nombrepeaje") ?? string.Empty,
                    Montopeaje = reader.GetNullableDecimal("montopeaje") ?? 0,
                    Activo = reader.GetNullableBool("Activo") ?? false
                };

                listaPeajes.Add(obj);
            }

            _logger.LogInformation("ListarPeajesPorZonas_SP OUT Count: {Count}", listaPeajes.Count);

            return listaPeajes;
        }
    }
}
