using Directo.Wari.TarifaEngine.Application.Features.Empresa.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class EmpresaRepository : SqlServerRepositoryBase, IEmpresaRepository
    {
        public EmpresaRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<int> ConstanteZona(int idEmpresa, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Empresa.EX1_USA_CONSTANTE_ZONA);

            SqlParameterHelper.AddParameter(command, "@idEmpresa", SqlDbType.Int, idEmpresa);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken)) return 0;

            return reader.GetInt32(reader.GetOrdinal("UsaConstanteZona"));
        }

        public async Task<bool> ValidarDia(int idEmpresa, DateTime fechaServicio, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection,SPName.Empresa.Validar_Dia_X1);

            SqlParameterHelper.AddParameter(command, "@idEmpresa", SqlDbType.Int, idEmpresa);
            SqlParameterHelper.AddParameter(command, "@fechaServicio", SqlDbType.DateTime, fechaServicio);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return false;

            if (!reader.HasColumn("Validar"))
                return false;

            var value = reader.GetNullableBool("Validar");

            return value ?? false;
        }

        public async Task<bool> ValidaConductorCercano(int idEmpresa, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Empresa.APP_VALIDAR_CONDUCTOR_CERCANO);

            SqlParameterHelper.AddParameter(command, "@idEmpresa", SqlDbType.Int, idEmpresa);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return false;

            if (!reader.HasColumn("ConductorCercano"))
                return false;

            return reader.GetNullableBool("ConductorCercano") ?? false;
        }
    }
}
