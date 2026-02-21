namespace Boilerplate.Api.Middleware
{
    public class ImpersonationTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public ImpersonationTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var impersonationToken = context.Request.Headers["Boilerplate_impersonated_token"].FirstOrDefault();

            if (!string.IsNullOrEmpty(impersonationToken))
            {
                context.Request.Headers["Authorization"] = $"Bearer {impersonationToken}";
            }

            await _next(context);
        }
    }
}
