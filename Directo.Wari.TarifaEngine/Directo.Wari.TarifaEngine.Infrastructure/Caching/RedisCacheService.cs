using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Directo.Wari.TarifaEngine.Infrastructure.Caching
{
    /// <summary>
    /// Implementación del servicio de caché usando Redis.
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var data = await _cache.GetStringAsync(key, cancellationToken);

            if (data is null)
                return default;

            return JsonSerializer.Deserialize<T>(data);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };

            var data = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, data, options, cancellationToken);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
    }
}
