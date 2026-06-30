using Directo.Wari.TarifaEngine.Application.Common.Util;
using Directo.Wari.TarifaEngine.Application.Features.Promociones.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Promociones.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Zona.Interfaces;
using Microsoft.Extensions.Logging;

namespace Directo.Wari.TarifaEngine.Application.Features.Promociones.Services
{
    public class PromocionesService : IPromocionesService
    {
        private readonly ILogger<PromocionesService> _logger;
        private readonly IPromocionesRepository _promocionesRepository;
        private readonly IZonaRepository _zonaRepository;

        public PromocionesService(IPromocionesRepository promocionesRepository, ILogger<PromocionesService> logger, IZonaRepository zonaRepository)
        {
            _promocionesRepository = promocionesRepository;
            _logger = logger;
            _zonaRepository = zonaRepository;
        }

        //TODO: ESTE METODO PARECE ESTA EN PRUEBA
        public async Task<Generic> ValidarPromo(PromoValidacionRequestDto json, CancellationToken cancellationToken)
        {
            var bean = new Generic();
            int idPromoActivacion = 0;

            // =========================
            // OBTENER PROMO
            // =========================
            if (json.IdPromocion <= 0)
            {
                var listPromocion = await _promocionesRepository.ObtenerPromocionCliente(json.IdCliente, cancellationToken);

                if (listPromocion?.Count > 0)
                {
                    var firstPromo = listPromocion[0];

                    var promocion = await _promocionesRepository.ObtenerPromocionClienteId(
                        json.IdCliente,
                        firstPromo.IdPromoActivacion,
                        json.TotalServicio,
                        cancellationToken);

                    idPromoActivacion = promocion == null ? 0 : firstPromo.IdPromoActivacion;
                    json.IdPromocion = firstPromo.IdPromocion;

                    _logger.LogInformation($"Se validará el siguiente idPromoActivacion {idPromoActivacion} al cliente {json.IdCliente}");
                }
            }
            else
            {
                idPromoActivacion = json.IdPromocionActivacion;
            }

            if (idPromoActivacion == 0)
            {
                return new Generic
                {
                    IdResultado = 2,
                    Value = "0"
                };
            }

            // =========================
            // ZONAS (PARALELO)
            // =========================
            var zonaOrigenTask = _zonaRepository.RecuperaByPosicion(json.IdEmpresa, json.OrigenLatitud, json.OrigenLongitud, cancellationToken);
            var zonaDestinoTask = _zonaRepository.RecuperaByPosicion(json.IdEmpresa, json.DestinoLatitud, json.DestinoLongitud, cancellationToken);

            await Task.WhenAll(zonaOrigenTask, zonaDestinoTask);

            var zonaOrigen = await zonaOrigenTask;
            var zonaDestino = await zonaDestinoTask;

            // =========================
            // VALIDAR PROMO
            // =========================
            var request = new ValidatePromocionRequestDto
            {
                ZonaOrigen = zonaOrigen,
                ZonaDestino = zonaDestino,
                TipoPago = json.TipoPago,
                TipoServicio = json.TipoServicio,
                IdCliente = json.IdCliente,
                IdEmpresa = json.IdEmpresa,
                FechaServicio = DateTimeHelper.ParseExact(json.FechaServicio!),
                IdPromoActivacion = idPromoActivacion
            };

            var validateResult = await _promocionesRepository.ValidatePromocion(request, cancellationToken);

            if (validateResult != null && validateResult.IdResultado > 0)
            {
                var (listZonaOrigen, listZonaDestino) = await _promocionesRepository.ObtenerZonasPromocion(json.IdPromocion, cancellationToken);

                // =========================
                // OPTIMIZACION O(n)
                // =========================
                var setDestino = new HashSet<int>(listZonaDestino);
                bool hayCoincidencia = listZonaOrigen.Any(z => setDestino.Contains(z));

                bool isZonaOrigen = listZonaOrigen.Contains(zonaOrigen);
                bool isZonaDestino = listZonaDestino.Contains(zonaDestino);

                bool isZonaOrigenAll = listZonaOrigen.Contains(-1);
                bool isZonaDestinoAll = listZonaDestino.Contains(-1);

                if (isZonaOrigenAll)
                    isZonaOrigen = true;

                if (isZonaDestinoAll)
                    isZonaDestino = true;

                if (!hayCoincidencia)
                {
                    if (isZonaOrigen && isZonaDestino)
                    {
                        bean.IdResultado = 2;
                        bean.Resultado = "Ok";
                    }
                    else
                    {
                        bean.IdResultado = -1;
                        bean.Resultado = "El punto origen o destino es diferente";
                    }
                }
                else
                {
                    if (isZonaOrigen || isZonaDestino)
                    {
                        bean.IdResultado = 2;
                        bean.Resultado = "Ok";
                    }
                    else
                    {
                        bean.IdResultado = -1;
                        bean.Resultado = "El punto origen o destino es igual";
                    }
                }
            }

            bean.Value = bean.IdResultado == 2 ? idPromoActivacion.ToString() : "0";

            return bean;
        }

        //TODO: CODIGO LEGACY PENDIENTE POR ELIMINAR
        public async Task<Generic> ValidarPromo_legacy(PromoValidacionRequestDto json, CancellationToken cancellationToken)
        {
            var bean = new Generic();
            int idPromoActivacion = 0;
            var promocion = new BeanPromocionAppResponseDto();
            var listPromocion = new List<BeanPromocionAppResponseDto>();

            if (json.IdPromocion <= 0)
            {
                listPromocion = await _promocionesRepository.ObtenerPromocionCliente(json.IdCliente, cancellationToken);
                if (listPromocion != null && listPromocion.Count > 0)
                {
                    promocion = await _promocionesRepository.ObtenerPromocionClienteId(json.IdCliente, listPromocion.First().IdPromoActivacion, json.TotalServicio, cancellationToken);
                    idPromoActivacion = promocion == null ? 0 : listPromocion.First().IdPromoActivacion;
                    json.IdPromocion = listPromocion.First().IdPromocion;
                    _logger.LogInformation($"Se validará el siguiente idPromoActivacion {idPromoActivacion} al cliente {json.IdCliente}");

                }
            }
            else
            {
                idPromoActivacion = json.IdPromocionActivacion;
            }

            if (idPromoActivacion == 0)
            {
                bean.IdResultado = 2;
                bean.Value = "0";
                return bean;
            }

            int zonaOrigen = await _zonaRepository.RecuperaByPosicion(json.IdEmpresa, json.OrigenLatitud, json.OrigenLongitud, cancellationToken);
            int zonaDestino = await _zonaRepository.RecuperaByPosicion(json.IdEmpresa, json.DestinoLatitud, json.DestinoLongitud, cancellationToken);

            var request = new ValidatePromocionRequestDto
            {
                ZonaOrigen = zonaOrigen,
                ZonaDestino = zonaDestino,
                TipoPago = json.TipoPago,
                TipoServicio = json.TipoServicio,
                IdCliente = json.IdCliente,
                IdEmpresa = json.IdEmpresa,
                FechaServicio = DateTimeHelper.ParseExact(json.FechaServicio!),
                IdPromoActivacion = idPromoActivacion
            };
            var ValidatePromocionResult = await _promocionesRepository.ValidatePromocion(request, cancellationToken);

            if (ValidatePromocionResult != null && ValidatePromocionResult.IdResultado > 0)
            {
                var (listZonaOrigen, listZonaDestino) = await _promocionesRepository.ObtenerZonasPromocion(json.IdPromocion, cancellationToken);

                bool hayCoincidencia = false;

                foreach (int elemento1 in listZonaOrigen)
                {
                    // Iterar sobre cada elemento de la segunda lista
                    foreach (int elemento2 in listZonaDestino)
                    {
                        // Si hay coincidencia, establecer la bandera y salir de los bucles
                        if (elemento1 == elemento2)
                        {
                            hayCoincidencia = true;
                            break;
                        }
                    }
                    if (hayCoincidencia)
                    {
                        break;
                    }
                }

                int isZonaOrigen = listZonaOrigen.Find(item => item == zonaOrigen);
                int isZonaDestino = listZonaDestino.Find(item => item == zonaDestino);

                int isZonaOrigenAll = listZonaOrigen.Find(item => item == -1);
                int isZonaDestinoAll = listZonaDestino.Find(item => item == -1);

                if (isZonaOrigenAll == -1)
                {
                    isZonaOrigen = 1;
                }
                else if (isZonaDestinoAll == -1)
                {
                    isZonaDestino = 1;
                }
                if (!hayCoincidencia)
                {
                    if (isZonaOrigen > 0 && isZonaDestino > 0)
                    {
                        bean.IdResultado = 2;
                        bean.Resultado = "Ok";
                    }
                    else
                    {
                        bean.IdResultado = -1;
                        bean.Resultado = "El punto origen o destino es diferente";
                    }

                }
                else
                {
                    // siempre y cuando una zona destino y origen coincidan
                    if (isZonaOrigen > 0 || isZonaDestino > 0)
                    {
                        bean.IdResultado = 2;
                        bean.Resultado = "Ok";
                    }
                    else
                    {
                        bean.IdResultado = -1;
                        bean.Resultado = "El punto origen o destino es igual";
                    }
                }
            }

            bean.Value = bean.IdResultado == 2 ? Convert.ToString(idPromoActivacion) : "0";

            return bean;
        }
    }
}
