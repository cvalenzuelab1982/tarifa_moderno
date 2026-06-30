using Directo.Wari.TarifaEngine.Application.Common.Options;
using Directo.Wari.TarifaEngine.Application.Features.HttpApi.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.HttpApi.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Parametros.Interfaces;
using Directo.Wari.TarifaEngine.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Directo.Wari.TarifaEngine.Infrastructure.Services.HttpApi
{
    public class HttpApiRepository : IHttpApiRepository
    {
        private readonly IParametrosRepository _parametrosRepository;
        private readonly ILogger<HttpApiRepository> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly HttpExternasOptions _httpExternasOptions;

        public HttpApiRepository(IParametrosRepository parametrosRepository, ILogger<HttpApiRepository> logger, HttpClient httpClient, IConfiguration configuration,  IOptions<HttpExternasOptions> httpExternasOptions)
        {
            _parametrosRepository = parametrosRepository;
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
            _httpExternasOptions = httpExternasOptions.Value;
        }

        public async Task<(decimal Distancia, string OverView, decimal Time)> ObtenerMetrajeDePuntosWithOverview(double origenLatitud, double origenLongitud, double destinoLatitud, double destinoLongitud, CancellationToken cancellationToken)
        {
            var urlBase = _httpExternasOptions.RUTA_API_MAPS_DIRECTION;
            var url = urlBase!
            .Replace("@origin", $"{origenLatitud},{origenLongitud}")
            .Replace("@destination", $"{destinoLatitud},{destinoLongitud}");
            var usarGoogle = false;
            var intento = 0;

            while (true)
            {
                intento++;

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    throw new BusinessRuleException("No se pudo consultar el servicio de rutas.");

                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                dynamic json = JsonConvert.DeserializeObject(jsonString)!;

                string status = json.status;

                if (status == "OK")
                {
                    if (json.routes.Count == 0)
                        return (0, string.Empty, 0);

                    var distancias = new List<decimal>();
                    var overviewPoints = new Dictionary<decimal, string>();
                    var tiempos = new Dictionary<decimal, decimal>();

                    foreach (var route in json.routes)
                    {
                        decimal distanciaTmp = route.legs[0].distance.value;
                        decimal tiempoTmp = route.legs[0].duration.value;
                        string overviewTmp = route.overview_polyline.points;

                        if (!overviewPoints.ContainsKey(distanciaTmp))
                        {
                            distancias.Add(distanciaTmp);
                            overviewPoints.Add(distanciaTmp, overviewTmp);
                            tiempos.Add(distanciaTmp, tiempoTmp);
                        }
                    }

                    distancias.Sort();

                    var distancia = distancias[0];

                    distancia = distancia * 0.001m;
                    distancia = Math.Truncate(100 * distancia) / 100;
                    distancia = distancia * 1000;

                    return (
                        distancia,
                        overviewPoints[distancias[0]],
                        tiempos[distancias[0]]
                    );
                }

                if (status == "UNKNOWN_ERROR" && url.Contains("HEREMAP"))
                {
                    var usarGmap = await _parametrosRepository.GetParameterValue("BUSCAR_DISTANCIA_GMAP", cancellationToken);

                    if (usarGmap == "1" && !usarGoogle)
                    {
                        usarGoogle = true;

                        url = url.Replace("HEREMAP", "GMAP");

                        _logger.LogInformation("Fallback a GMAP para cálculo de distancia. Url: {url}", url);

                        intento = 0;
                        continue;
                    }
                }

                if (status == "OVER_QUERY_LIMIT")
                {
                    if (intento >= 3)
                        throw new BusinessRuleException($"No se pudo calcular la distancia. Error: {status}");

                    continue;
                }

                throw new BusinessRuleException($"No se pudo calcular la distancia. Error: {status}");
            }
        }

        public async Task<(decimal Distancia, string OverView, decimal Time)> ObtenerMetrajeDePuntosWithOverviewWayPoint(double origenLatitud, double origenLongitud, double destinoLatitud, double destinoLongitud, List<SrvDestinoResponseDto> lstDestino, CancellationToken cancellationToken)
        {
            string url;

            if (lstDestino != null && lstDestino.Count > 0)
            {
                var urlBase = _httpExternasOptions.RUTA_API_MAPS_DIRECTION_WAYPOINTS;

                var origen = $"{lstDestino.First().origenLatitud},{lstDestino.First().origenLongitud}";
                var destino = $"{lstDestino.Last().destinoLatitud},{lstDestino.Last().destinoLongitud}";

                var vias = string.Join("|",
                    lstDestino
                    .Skip(1)
                    .Take(lstDestino.Count - 2)
                    .Select(x => $"{x.destinoLatitud},{x.destinoLongitud}")
                );

                url = urlBase
                    .Replace("@ORIGEN", origen)
                    .Replace("@DESTINO", destino)
                    .Replace("@VIA", vias);
            }
            else
            {
                var urlBase = _httpExternasOptions.RUTA_API_MAPS_DIRECTION;

                url = urlBase
                    .Replace("@origin", $"{origenLatitud},{origenLongitud}")
                    .Replace("@destination", $"{destinoLatitud},{destinoLongitud}");
            }

            _logger.LogInformation("Consulta distancia rutas url: {url}", url);

            bool usarGoogle = false;
            int intento = 0;

            while (true)
            {
                intento++;

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    throw new BusinessRuleException("No se pudo consultar el servicio de rutas.");

                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);

                dynamic json = JsonConvert.DeserializeObject(jsonString)!;

                string status = json.status;

                if (status == "OK")
                {
                    if (json.routes.Count == 0)
                        return (0, string.Empty, 0);

                    var distancias = new List<decimal>();
                    var overviewPoints = new Dictionary<decimal, string>();
                    var tiempos = new Dictionary<decimal, decimal>();

                    foreach (var route in json.routes)
                    {
                        decimal distanciaTmp = 0;
                        decimal tiempoTmp = 0;

                        foreach (var leg in route.legs)
                        {
                            distanciaTmp += Convert.ToDecimal(leg.distance.value);
                            tiempoTmp += Convert.ToDecimal(leg.duration.value);
                        }

                        string overviewTmp = route.overview_polyline.points;

                        if (!overviewPoints.ContainsKey(distanciaTmp))
                        {
                            distancias.Add(distanciaTmp);
                            overviewPoints.Add(distanciaTmp, overviewTmp);
                            tiempos.Add(distanciaTmp, tiempoTmp);
                        }
                    }

                    distancias.Sort();

                    var distancia = distancias[0];

                    return (
                        distancia,
                        overviewPoints[distancia],
                        tiempos[distancia]
                    );
                }

                if (status == "UNKNOWN_ERROR" && url.Contains("HEREMAP"))
                {
                    var usarGmap = await _parametrosRepository.GetParameterValue("BUSCAR_DISTANCIA_GMAP", cancellationToken);

                    if (usarGmap == "1" && !usarGoogle)
                    {
                        usarGoogle = true;

                        url = url.Replace("HEREMAP", "GMAP");

                        _logger.LogInformation("Fallback a GMAP para cálculo de distancia. Url: {url}", url);

                        intento = 0;
                        continue;
                    }
                }

                if (status == "OVER_QUERY_LIMIT")
                {
                    if (intento >= 3)
                        throw new BusinessRuleException($"No se pudo calcular la distancia. error: {status}");

                    continue;
                }

                throw new BusinessRuleException($"No se pudo calcular la distancia. error: {status}");
            }
        }

    }
}
