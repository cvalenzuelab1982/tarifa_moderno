using System.Globalization;

namespace Directo.Wari.TarifaEngine.Infrastructure.Persistence.Helpers
{
    /// <summary>
    /// Normaliza lat/lng y construye una key de cache consistente.
    /// Evita que pequeñas diferencias en coordenadas rompan el cache.
    /// </summary>
    public static class GeoCacheKeyHelper
    {
        private const int DefaultDecimals = 5;

        public static string Build(
            string prefix,
            double lat,
            double lng,
            params object[] extraParts)
        {
            var latNorm = lat.ToString($"F{DefaultDecimals}", CultureInfo.InvariantCulture);
            var lngNorm = lng.ToString($"F{DefaultDecimals}", CultureInfo.InvariantCulture);

            var key = $"{prefix}_{latNorm}_{lngNorm}";

            if (extraParts != null && extraParts.Length > 0)
            {
                key += "_" + string.Join("_", extraParts);
            }

            return key;
        }
    }
}
