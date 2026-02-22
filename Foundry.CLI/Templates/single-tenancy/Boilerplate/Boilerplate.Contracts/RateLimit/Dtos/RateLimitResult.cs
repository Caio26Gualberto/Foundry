namespace Boilerplate.Contracts.RateLimit.Dtos
{
    public sealed record RateLimitResult(
        bool Allowed,
        int Remaining,
        DateTime ResetAtUtc
    );
}
