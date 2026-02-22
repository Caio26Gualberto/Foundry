using Boilerplate.Contracts.RateLimit.Dtos;

namespace Boilerplate.Contracts.RateLimit.Interface
{
    public interface IRateLimitService
    {
        Task<RateLimitResult> CheckAsync(string key, int limit, TimeSpan window);
    }
}
