using Directo.Wari.TarifaEngine.Application.Common.Models;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;
using MediatR;

namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Queries.SolicitarPrecios
{
    public sealed record SolicitarPreciosQuery(TarifaRequestDto request) : IRequest<Result<TarifaResponseDto>>;
}
