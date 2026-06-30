using Directo.Wari.TarifaEngine.Application.Features.Plaza.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class PlazaRepository : SqlServerRepositoryBase, IPlazaRepository
    {
        public PlazaRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<decimal> RecalculoTarifaPlaza(int idZonaOrigen, int idZonaDestino, DateTime fechaServicio, CancellationToken cancellationToken)
        {
            decimal monto = 0;

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Plaza.TARIFA_HORARIO_PLAZA);

            SqlParameterHelper.AddParameter(command, "@IdZonaOrigen", SqlDbType.Int, idZonaOrigen);
            SqlParameterHelper.AddParameter(command, "@IdZonaDestino", SqlDbType.Int, idZonaDestino);
            SqlParameterHelper.AddParameter(command, "@FechaServicio", SqlDbType.DateTime, fechaServicio);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                monto = reader.HasColumn("Monto")
                    ? reader.GetNullableDecimal("Monto") ?? 0
                    : 0;
            }

            return monto;
        }

        public async Task<bool> ValidarZonaPlaza(double latitud, double longitud, int idZona, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Plaza.X1_WS_VERIFICAR_PUNTO_ZONA_PLAZA);

            SqlParameterHelper.AddParameter(command, "@Latitud", SqlDbType.Float, latitud);
            SqlParameterHelper.AddParameter(command, "@Longitud", SqlDbType.Float, longitud);
            SqlParameterHelper.AddParameter(command, "@idZona", SqlDbType.Int, idZona);

            await connection.OpenAsync(cancellationToken);

            var result = await command.ExecuteScalarAsync(cancellationToken);

            return result != null;
        }
    }
}
