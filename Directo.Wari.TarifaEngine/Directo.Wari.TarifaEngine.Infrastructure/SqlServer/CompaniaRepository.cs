using Directo.Wari.TarifaEngine.Application.Features.Compania.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class CompaniaRepository : SqlServerRepositoryBase, ICompaniaRepository
    {
        public CompaniaRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<int> RecuperarCompania(
             int idEmpresa,
             double latitudOrigen,
             double longitudOrigen,
             double latitudDestino,
             double longitudDestino,
            CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Compania.EX1_WS_RECUPERAR_COMPANIA);

            SqlParameterHelper.AddParameter(command, "@IdEmpresa", SqlDbType.Int, idEmpresa);
            SqlParameterHelper.AddParameter(command, "@Origen_Latitud", SqlDbType.Float, latitudOrigen);
            SqlParameterHelper.AddParameter(command, "@Origen_Longitud", SqlDbType.Float, longitudOrigen);
            SqlParameterHelper.AddParameter(command, "@Destino_Latitud", SqlDbType.Float, latitudDestino);
            SqlParameterHelper.AddParameter(command, "@Destino_Longitud", SqlDbType.Float, longitudDestino);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                // primera columna (como item[0])
                return reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
            }

            return 0;
        }
    }
}
