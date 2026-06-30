using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.RecargoReserva.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.RecargoReserva.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class RecargoReservaRepository : SqlServerRepositoryBase, IRecargoReservaRepository
    {
        public RecargoReservaRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<BeanRecargoReservaResponseDto?> GetRecargoReservaOnValue(int idTipoPago, TarifaDestinoRequestDto beanServicio, TarifaDetalleResponseDto beanTarifa, DateTime FechaServicio, decimal totalTarifaBase = 0, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.RecargoReserva.X2_VALIDAR_HORA_PUNTA_MODO_RESERVA);

            SqlParameterHelper.AddParameter(command, "@IdEmpresa", SqlDbType.Int, beanServicio.IdEmpresa);
            SqlParameterHelper.AddParameter(command, "@IdTipoServicio", SqlDbType.Int, beanServicio.TipoServicio);
            SqlParameterHelper.AddParameter(command, "@IdTipoPago", SqlDbType.Int, idTipoPago);
            SqlParameterHelper.AddParameter(command, "@Total", SqlDbType.Money, beanTarifa.TarifaBase + totalTarifaBase);
            SqlParameterHelper.AddParameter(command, "@Anticipada", SqlDbType.Bit, beanServicio.Anticipada);
            SqlParameterHelper.AddParameter(command, "@AnticipadoAlMomento", SqlDbType.Bit, beanServicio.Anticipadoalmomento);
            SqlParameterHelper.AddParameter(command, "@FechaServicio", SqlDbType.DateTime, FechaServicio);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return Map(reader);
        }

        private BeanRecargoReservaResponseDto Map(SqlDataReader reader)
        {
            return new BeanRecargoReservaResponseDto
            {
                IdRecargoReserva = reader.GetNullableInt("idRecargoEspecial"),
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
