using Directo.Wari.TarifaEngine.Application.Features.Cliente.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class ClienteRepository : SqlServerRepositoryBase, IClienteRepository
    {
        public ClienteRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<bool> ServiciosPorCalificar(int idCliente, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Cliente.APP_VALIDAR_SERVICIOS_POR_CALIFICAR);

            SqlParameterHelper.AddParameter(command, "@idCliente", SqlDbType.Int, idCliente);

            var result = await command.ExecuteScalarAsync(cancellationToken);

            if (result == null)
                return false;

            return Convert.ToInt32(result) > 0;

        }

        public async Task<Dictionary<string, string>?> ObtenerePresupuestoCliente(int idCliente, CancellationToken cancellationToken)
        {
            Dictionary<string, string>? presupuestoAlo = null;

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Cliente.Presupuesto_Saldo);

            SqlParameterHelper.AddParameter(command, "@idCliente", SqlDbType.Int, idCliente);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var presupuestoMensual = reader.GetNullableDecimal("presupuestoMensual") ?? 0;
                var presupuestoPendiente = reader.GetNullableDecimal("presupuestoPendiente") ?? 0;
                var servicioMensual = reader.GetNullableDecimal("servicioMensual") ?? 0;
                var servicioPendiente = reader.GetNullableInt("servicioPendiente") ?? 0;

                presupuestoAlo = new Dictionary<string, string>
                {
                    ["PRESUPUESTO_MENSUAL"] = presupuestoMensual.ToString(),
                    ["PRESUPUESTO_PENDIENTE"] = presupuestoPendiente.ToString(),
                    ["SERVICIO_MENSUAL"] = servicioMensual.ToString(),
                    ["SERVICIO_PENDIENTE"] = servicioPendiente.ToString()
                };
            }

            return presupuestoAlo;
        }
    }
}
