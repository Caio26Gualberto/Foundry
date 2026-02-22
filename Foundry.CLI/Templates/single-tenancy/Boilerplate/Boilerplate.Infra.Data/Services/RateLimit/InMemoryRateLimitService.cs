using Boilerplate.Contracts.RateLimit.Dtos;
using Boilerplate.Contracts.RateLimit.Interface;

public sealed class InMemoryRateLimitService : IRateLimitService
{
    private static readonly Dictionary<string, (int Count, DateTime Expiration)> _store
        = new();

    private static readonly object _lock = new();

    public Task<RateLimitResult> CheckAsync(
        string key,
        int limit,
        TimeSpan window)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;

            if (_store.TryGetValue(key, out var entry))
            {
                if (entry.Expiration < now)
                {
                    _store[key] = (1, now.Add(window));
                }
                else
                {
                    _store[key] = (entry.Count + 1, entry.Expiration);
                }
            }
            else
            {
                _store[key] = (1, now.Add(window));
            }

            var current = _store[key];

            var remaining = Math.Max(0, limit - current.Count);
            var allowed = current.Count <= limit;

            return Task.FromResult(
                new RateLimitResult(
                    allowed,
                    remaining,
                    current.Expiration));
        }
    }
}