namespace Directo.Wari.TarifaEngine.Application.Common.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de fecha/hora (facilita testing).
    /// </summary>
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }

    /// <summary>
    /// Interfaz para obtener información del usuario actual.
    /// </summary>
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        bool IsAuthenticated { get; }
    }

    /// <summary>
    /// Interfaz para el servicio de caché.
    /// </summary>
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    }
}
