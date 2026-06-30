using Directo.Wari.TarifaEngine.Application.Features.Promociones.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Promociones.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Constants;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers;
using Directo.Wari.TarifaEngine.Infrastructure.SqlServer.Base;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Directo.Wari.TarifaEngine.Infrastructure.SqlServer
{
    public class PromocionesRepository : SqlServerRepositoryBase, IPromocionesRepository
    {
        public PromocionesRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<List<BeanPromocionAppResponseDto>> ObtenerPromocionCliente(int idCliente, CancellationToken cancellationToken)
        {
            var result = new List<BeanPromocionAppResponseDto>();

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Promociones.X9_PromocionVigencia);

            SqlParameterHelper.AddParameter(command, "@IdCliente", SqlDbType.Int, idCliente);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(MapPromocion(reader));
            }

            return result;
        }

        private BeanPromocionAppResponseDto MapPromocion(SqlDataReader reader)
        {
            return new BeanPromocionAppResponseDto
            {
                IdPromoActivacion = reader.GetNullableInt("idpromoactivacion") ?? 0,
                IdPromocion = reader.GetNullableInt("idPromocion") ?? 0,
                Codigo = reader.GetNullableString("codigo") ?? string.Empty,
                Nombre = reader.GetNullableString("nombre") ?? string.Empty,
                Descripcion = reader.GetNullableString("descripcion") ?? string.Empty,

                FechaActivado = reader.HasColumn("Fecha") && !reader.IsDBNull(reader.GetOrdinal("Fecha"))
                    ? reader.GetDateTime(reader.GetOrdinal("Fecha"))
                    : DateTime.MinValue,

                FechaInicio = reader.HasColumn("fechaInicio") && !reader.IsDBNull(reader.GetOrdinal("fechaInicio"))
                    ? reader.GetDateTime(reader.GetOrdinal("fechaInicio"))
                    : DateTime.MinValue,

                FechaFin = reader.HasColumn("fechaFin") && !reader.IsDBNull(reader.GetOrdinal("fechaFin"))
                    ? reader.GetDateTime(reader.GetOrdinal("fechaFin"))
                    : DateTime.MinValue,

                ModalidadPromocion = reader.GetNullableInt("I057_ModalidadPromocion") ?? 0,
                ValorPromocion = reader.GetNullableDecimal("ValorPromocion") ?? 0,

                UrlPromocion = reader.GetNullableString("urlPromocion") ?? string.Empty,
                UrlImagen = reader.GetNullableString("urlImagen") ?? string.Empty,

                IsPrecargado = reader.GetNullableInt("isPrecargado") ?? 0,

                Consumido = reader.HasColumn("consumido") ? reader.GetNullableInt("consumido") ?? 0 : 0,
                Cantidad = reader.HasColumn("cantidad") ? reader.GetNullableInt("cantidad") ?? 0 : 0,
                Disponible = reader.HasColumn("disponible") ? reader.GetNullableInt("disponible") ?? 0 : 0,

                IsAgotarValor = reader.HasColumn("isAgotarValor") && (reader.GetNullableBool("isAgotarValor") ?? false),
                IsClienteNuevo = reader.HasColumn("isClienteNuevo") && (reader.GetNullableBool("isClienteNuevo") ?? false),
                ValorConsumido = reader.HasColumn("valorConsumido") ? reader.GetNullableDecimal("valorConsumido") ?? 0 : 0
            };
        }

        public async Task<BeanPromocionAppResponseDto?> ObtenerPromocionClienteId(int idCliente, int idPromocion, decimal totalServicio = 0, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Promociones.X10_PromocionVigenciaId);

            SqlParameterHelper.AddParameter(command, "@IdCliente", SqlDbType.Int, idCliente);
            SqlParameterHelper.AddParameter(command, "@IdPromoActivado", SqlDbType.Int, idPromocion);
            SqlParameterHelper.AddParameter(command, "@totalServicio", SqlDbType.Decimal, totalServicio);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            return MapPromocionClienteId(reader);
        }

        private BeanPromocionAppResponseDto MapPromocionClienteId(SqlDataReader reader)
        {
            return new BeanPromocionAppResponseDto
            {
                IdPromoActivacion = reader.GetNullableInt("idpromoactivacion") ?? 0,
                IdPromocion = reader.GetNullableInt("idPromocion") ?? 0,

                Codigo = reader.GetNullableString("codigo") ?? string.Empty,
                Nombre = reader.GetNullableString("nombre") ?? string.Empty,
                Descripcion = reader.GetNullableString("descripcion") ?? string.Empty,

                FechaActivado = reader.HasColumn("Fecha")
                    ? (reader.IsDBNull(reader.GetOrdinal("Fecha"))
                        ? DateTime.MinValue
                        : reader.GetDateTime(reader.GetOrdinal("Fecha")))
                    : DateTime.MinValue,

                FechaInicio = reader.HasColumn("fechaInicio")
                    ? (reader.IsDBNull(reader.GetOrdinal("fechaInicio"))
                        ? DateTime.MinValue
                        : reader.GetDateTime(reader.GetOrdinal("fechaInicio")))
                    : DateTime.MinValue,

                FechaFin = reader.HasColumn("fechaFin")
                    ? (reader.IsDBNull(reader.GetOrdinal("fechaFin"))
                        ? DateTime.MinValue
                        : reader.GetDateTime(reader.GetOrdinal("fechaFin")))
                    : DateTime.MinValue,

                ModalidadPromocion = reader.GetNullableInt("I057_ModalidadPromocion") ?? 0,
                ValorPromocion = reader.GetNullableDecimal("ValorPromocion") ?? 0,

                UrlPromocion = reader.GetNullableString("urlPromocion") ?? string.Empty,
                UrlImagen = reader.GetNullableString("urlImagen") ?? string.Empty,

                IsAgotarValor = reader.HasColumn("isAgotarValor") && (reader.GetNullableBool("isAgotarValor") ?? false),
                ValorConsumido = reader.HasColumn("valorConsumido") ? reader.GetNullableDecimal("valorConsumido") ?? 0 : 0
            };
        }

        public async Task<(List<int> ZonaOrigen, List<int> ZonaDestino)> ObtenerZonasPromocion(int idPromocion, CancellationToken cancellationToken)
        {
            var zonaOrigen = new List<int>();
            var zonaDestino = new List<int>();

            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Promociones.SP_GET_ZONAS_PARAMETRIZADOS_PROMOCION_x1);

            SqlParameterHelper.AddParameter(command, "@IDPROMOCION", SqlDbType.Int, idPromocion);

            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            // Tabla 1: origen
            while (await reader.ReadAsync(cancellationToken))
            {
                zonaOrigen.Add(reader.GetNullableInt("Id") ?? 0);
            }

            // Tabla 2: destino
            if (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    zonaDestino.Add(reader.GetNullableInt("Id") ?? 0);
                }
            }

            return (zonaOrigen, zonaDestino);
        }

        public async Task<ValidatePromocionResponseDto?> ValidatePromocion(ValidatePromocionRequestDto request, CancellationToken cancellationToken)
        {
            await using var connection = CreateConnection();
            await using var command = CreateStoredProcedure(connection, SPName.Promociones.API_VALIDATE_PROMOCION_X2_Prueba);

            SqlParameterHelper.AddParameter(command, "@zonaOrigen", SqlDbType.Int, request.ZonaOrigen);
            SqlParameterHelper.AddParameter(command, "@zonaDestino", SqlDbType.Int, request.ZonaDestino);
            SqlParameterHelper.AddParameter(command, "@tipoPago", SqlDbType.Int, request.TipoPago);
            SqlParameterHelper.AddParameter(command, "@tipoServicio", SqlDbType.Int, request.TipoServicio);
            SqlParameterHelper.AddParameter(command, "@idCliente", SqlDbType.Int, request.IdCliente);
            SqlParameterHelper.AddParameter(command, "@idEmpresa", SqlDbType.Int, request.IdEmpresa);
            SqlParameterHelper.AddParameter(command, "@fechaServicio", SqlDbType.DateTime, request.FechaServicio);
            SqlParameterHelper.AddParameter(command, "@idPromoActivacion", SqlDbType.Int, request.IdPromoActivacion);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return MapValidatePromocion(reader);
        }

        private ValidatePromocionResponseDto MapValidatePromocion(SqlDataReader reader)
        {
            return new ValidatePromocionResponseDto
            {
                IdResultado = reader.GetInt32("idResultado"),
                Resultado = reader.GetNullableString("Resultado") ?? string.Empty
            };
        }
    }
}
