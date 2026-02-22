namespace Boilerplate.Api.RateLimiting
{
    public interface IRateLimitPolicy
    {
        int Limit { get; }
        TimeSpan Window { get; }
        string BuildKey(HttpContext context);
    }
}
