using Directo.Wari.TarifaEngine.Domain.Aggregates;

namespace Directo.Wari.TarifaEngine.Application.Common.Util
{
    public static class Utilitaries
    {
        public static List<int> GetListaDeEmpresas(string? parametro)
        {
            if (string.IsNullOrWhiteSpace(parametro))
                return [];

            return parametro
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(int.Parse)
                .ToList();
        }

        public static bool TienePropiedad(List<ConfiguracionZona> config, int idZona, string propiedad)
        {
            return config.Any(x =>
                x.IdZona == idZona &&
                x.Propiedad == propiedad &&
                x.Valor);
        }
    }
}
