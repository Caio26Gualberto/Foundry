using Boilerplate.Contracts.RateLimit.Dtos;
using Boilerplate.Contracts.RateLimit.Interface;
using Boilerplate.Infra.Data.Redis;

namespace Boilerplate.Infra.Data.Services.RateLimit
{
    public sealed class RateLimitService : IRateLimitService
    {
        private readonly IRedisConnection _redis;
        private readonly string _prefix;
        public RateLimitService(IRedisConnection redisConnection)
        {
            _redis = redisConnection;
        }

        public async Task<RateLimitResult> CheckAsync(string key, int limit, TimeSpan window)
        {
            var redisKey = $"{_prefix}ratelimit:{key}";
            var db = _redis.Database;

            var current = await db.StringIncrementAsync(redisKey);

            if (current == 1)
                await db.KeyExpireAsync(redisKey, window);

            var ttl = await db.KeyTimeToLiveAsync(redisKey);

            var remaining = Math.Max(0, limit - (int)current);
            var allowed = current <= limit;

            return new RateLimitResult
            (
                allowed,
                remaining,
                DateTime.UtcNow.Add(ttl ?? TimeSpan.Zero)
            );
        }
    }
}
