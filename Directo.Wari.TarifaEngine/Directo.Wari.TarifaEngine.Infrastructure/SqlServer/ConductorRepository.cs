using Directo.Wari.TarifaEngine.Application.Features.Conductor.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class ConductorRepository : SqlServerRepositoryBase, IConductorRepository
    {
        public ConductorRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<bool> ExisteConductoresCercanos(double Longitud,double Latitud,int IdEmpresa, int IdCliente,CancellationToken cancellationToken)
        {
            bool response = false;

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Conductor.EX1_USA_CONSTANTE_ZONA);

            SqlParameterHelper.AddParameter(command, "@Pos_Origen_Longitud", SqlDbType.Float, Longitud);
            SqlParameterHelper.AddParameter(command, "@Pos_Origen_Latitud", SqlDbType.Float, Latitud);
            SqlParameterHelper.AddParameter(command, "@IdEmpresa", SqlDbType.Int, IdEmpresa);
            SqlParameterHelper.AddParameter(command, "@IdCliente", SqlDbType.Int, IdCliente);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            // Ir al último resultset
            do
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    if (reader.HasColumn("FILAS"))
                    {
                        var filas = reader.GetNullableInt("FILAS") ?? 0;

                        if (filas > 0)
                        {
                            response = true;
                        }
                    }
                }
            }
            while (await reader.NextResultAsync(cancellationToken));

            return response;
        }
    }
}
