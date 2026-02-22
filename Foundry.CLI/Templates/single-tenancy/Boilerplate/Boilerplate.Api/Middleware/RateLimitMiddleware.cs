using Boilerplate.Api.RateLimiting;
using Boilerplate.Contracts.RateLimit.Interface;
using Microsoft.AspNetCore.RateLimiting;

namespace Boilerplate.Api.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        public RateLimitMiddleware(RequestDelegate requestDelegate)
        {
            _next = requestDelegate;
        }

        public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimit, IServiceProvider provider)
        {
            var endpoint = context.GetEndpoint();
            var attribute = endpoint?.Metadata.GetMetadata<RateLimitAttribute>();

            if (attribute is null)
            {
                await _next(context);
                return;
            }

            var policy = (IRateLimitPolicy)provider.GetRequiredService(attribute.PolicyType);
            var key = policy.BuildKey(context);

            var result = await rateLimit.CheckAsync(key, policy.Limit, policy.Window);

            if (!result.Allowed)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too many requests.");
                return;
            }

            await _next(context);
        }
    }
}
