namespace Directo.Wari.TarifaEngine.Application.Common.Models
{
    /// <summary>
    /// Permite tratar valores NULLOS como Si exite pero NULL, para evitar estar consultado la DB
    /// </summary>
    public class CacheItem<T>
    {
        public bool HasValue { get; set; }
        public T? Value { get; set; }
    }
}
