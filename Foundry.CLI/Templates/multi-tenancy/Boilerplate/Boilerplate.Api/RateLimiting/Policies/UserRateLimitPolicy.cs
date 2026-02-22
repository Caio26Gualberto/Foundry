using System.Security.Claims;

namespace Boilerplate.Api.RateLimiting.Policies
{
    public class UserRateLimitPolicy : IRateLimitPolicy
    {
        public int Limit => 120;

        public TimeSpan Window => TimeSpan.FromMinutes(1);

        public string BuildKey(HttpContext context)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return $"user:{userId}";
        }
    }
}
