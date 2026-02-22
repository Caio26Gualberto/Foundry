using Boilerplate.Infra.Data.Redis;
using System.Text.Json;

namespace Boilerplate.Infra.Data.Services.Caching
{
    public sealed class CacheService : ICacheService
    {
        private readonly IRedisConnection _connection;
        public CacheService(IRedisConnection redisConnection)
        {
            _connection = redisConnection;
        }

        public async Task<T?> GetTAsync<T>(string key)
        {
            var value = await _connection.Database.StringGetAsync(key);

            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task RemoveAsync(string key)
        {
            await _connection.Database.KeyDeleteAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var serializedValue = JsonSerializer.Serialize(value);

            await _connection.Database.StringSetAsync(key, serializedValue, expiration);
        }
    }
}
