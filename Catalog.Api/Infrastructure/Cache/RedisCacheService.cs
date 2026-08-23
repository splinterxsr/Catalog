using StackExchange.Redis;
using System.Text.Json;

namespace Catalog.Api.Infrastructure.Cache
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _db;
        private readonly TimeSpan _defaultTtl;

        public RedisCacheService(IConnectionMultiplexer multiplexer)
        {
            _db = multiplexer.GetDatabase();
            _defaultTtl = TimeSpan.FromSeconds(300);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var val = await _db.StringGetAsync(key);
            if (!val.HasValue) return default;
            return JsonSerializer.Deserialize<T>(val.ToString());
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, ttl ?? _defaultTtl);
        }

        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }
    }
}