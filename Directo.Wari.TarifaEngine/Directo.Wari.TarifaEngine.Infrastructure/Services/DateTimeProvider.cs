using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Directo.Wari.TarifaEngine.Infrastructure.Services
{
    /// <summary>
    /// Proveedor de fecha/hora del sistema.
    /// </summary>
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
