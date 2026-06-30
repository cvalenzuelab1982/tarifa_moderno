using Directo.Wari.TarifaEngine.Application.Features.HoraPunta.Dto;
using Directo.Wari.TarifaEngine.Application.Features.HoraPunta.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class HoraPuntaRepository : SqlServerRepositoryBase, IHoraPuntaRepository
    {
        public HoraPuntaRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<BeanHoraPuntaResponseDto?> GetHoraPuntaOnValue(int idEmpresa, int tipopago, int idTipoServicio, DateTime FechaServicio, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.HoraPunta.X3_VALIDAR_HORA_PUNTA);

            SqlParameterHelper.AddParameter(command, "@IdEmpresa", SqlDbType.Int, idEmpresa);
            SqlParameterHelper.AddParameter(command, "@IdTipoServicio", SqlDbType.Int, idTipoServicio);
            SqlParameterHelper.AddParameter(command, "@IdTipoPago", SqlDbType.Int, tipopago);
            SqlParameterHelper.AddParameter(command, "@FechaServicio", SqlDbType.DateTime, FechaServicio);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return Map(reader);
        }

        private BeanHoraPuntaResponseDto Map(SqlDataReader reader)
        {
            return new BeanHoraPuntaResponseDto
            {
                IdHoraPunta = reader.GetNullableInt("idHoraPunta"),
                IdEmpresa = reader.GetNullableInt("idEmpresa"),
                IdTipoServicio = reader.GetNullableInt("idTipoServicio"),
                HoraInicio = reader.GetTimeSpan(reader.GetOrdinal("HoraInicio")),
                HoraFin = reader.GetTimeSpan(reader.GetOrdinal("HoraFin")),
                Incremento = reader.GetDecimal("Incremento"),
                Activo = reader.GetNullableBool("Activo"),
                TipoCalculo = reader.GetInt32("TipoCalculo")
            };
        }
    }
}
