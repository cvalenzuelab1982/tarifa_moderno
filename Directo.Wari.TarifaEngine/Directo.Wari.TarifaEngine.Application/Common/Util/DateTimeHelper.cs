using Directo.Wari.TarifaEngine.Domain.Common;
using Directo.Wari.TarifaEngine.Domain.Exceptions;
using System.Globalization;

namespace Directo.Wari.TarifaEngine.Application.Common.Util
{
    public static class DateTimeHelper
    {
        private const string DefaultFormat = "dd/MM/yyyy HH:mm:ss";

        public static DateTime ParseExact(string fecha)
        {
            if (string.IsNullOrWhiteSpace(fecha))
                throw new BusinessRuleException("La fecha no puede ser nula o vacía.");

            if (!DateTime.TryParseExact(
                    fecha,
                    DefaultFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var resultado))
            {
                throw new BusinessRuleException($"Formato de fecha inválido. Formato esperado: {DefaultFormat}");
            }

            return resultado;
        }
    }
}
