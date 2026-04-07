using Boilerplate.Api.ApiResponse;
using System.Net.Sockets;
using System.Security;
using System.Security.Authentication;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                // 401 - Problemas de autenticação/autorização técnica
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                AuthenticationException => StatusCodes.Status401Unauthorized,
                SecurityException => StatusCodes.Status401Unauthorized,

                // Timeout / cancelamento (infra / rede)
                TaskCanceledException => StatusCodes.Status503ServiceUnavailable,
                TimeoutException => StatusCodes.Status503ServiceUnavailable,
                OperationCanceledException => StatusCodes.Status503ServiceUnavailable,

                // Falhas de integração externa / rede
                SocketException => StatusCodes.Status502BadGateway,
                HttpRequestException httpEx when httpEx.Message.Contains("429")
                    => StatusCodes.Status429TooManyRequests,
                HttpRequestException => StatusCodes.Status502BadGateway,

                // Infra geral (IO, disco, etc.)
                IOException => StatusCodes.Status503ServiceUnavailable,

                // fallback — erro inesperado
                _ => StatusCodes.Status500InternalServerError
            };

            var response = new BoilerplateResponse<object>
            {
                IsSuccess = false,
                Message = ex.Message
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
