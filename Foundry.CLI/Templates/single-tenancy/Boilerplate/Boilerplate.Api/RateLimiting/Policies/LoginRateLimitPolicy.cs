namespace Boilerplate.Api.RateLimiting.Policies
{
    public class LoginRateLimitPolicy : IRateLimitPolicy
    {
        public int Limit => 10;
        public TimeSpan Window => TimeSpan.FromMinutes(1);

        public string BuildKey(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            return $"login:ip:{ip}";
        }
    }
}
