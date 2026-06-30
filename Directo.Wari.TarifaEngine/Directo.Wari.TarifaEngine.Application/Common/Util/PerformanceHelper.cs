using System.Diagnostics;

namespace Directo.Wari.TarifaEngine.Application.Common.Util
{
    public static class PerformanceHelper
    {
        /// <summary>
        /// Permite obtener el tiempo para un proceso de pruebas
        /// </summary>
        /// <param name="nombre"></param>
        /// <param name="accion"></param>
        /// <returns></returns>
        public static async Task MedirAsync(string nombre, Func<Task> accion)
        {
            var sw = Stopwatch.StartNew();

            await accion();

            sw.Stop();
            Console.WriteLine($"{nombre}: {sw.ElapsedMilliseconds} ms");
        }
    }
}
