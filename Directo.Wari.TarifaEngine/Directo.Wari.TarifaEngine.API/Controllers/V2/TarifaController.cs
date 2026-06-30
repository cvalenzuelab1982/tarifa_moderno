using Asp.Versioning;
using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Queries.SolicitarPrecios;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Directo.Wari.TarifaEngine.API.Controllers.V2
{
    /// <summary>
    /// Controller para TarfiaAuthorization.
    /// </summary>
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/tarifaAuthorization")]
    public class TarifaController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public TarifaController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        [HttpPost("consultar")]
        public async Task<IActionResult> SolicitarPrecios([FromBody] TarifaRequestDto request)
        {
            var result = await _mediator.Send(new SolicitarPreciosQuery(request));
            var response = new TarifaResponseDto();

            if (!result.IsSuccess)
            {
                response.IdResultado = result.Error?.IdResultado ?? -1;
                response.Resultado = result.Error?.Message ?? "Se han producido uno o varios errores.";
                return BadRequest(response);
            }

            return Ok(result.Value);
        }
    }
}
