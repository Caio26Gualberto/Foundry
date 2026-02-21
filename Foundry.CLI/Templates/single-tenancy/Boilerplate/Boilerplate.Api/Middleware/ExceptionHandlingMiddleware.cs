using Boilerplate.Api.ApiResponse;
using System.ComponentModel.DataAnnotations;
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
                // 400 - Requisição inválida (erros de input)
                ArgumentNullException => StatusCodes.Status400BadRequest,
                ArgumentException => StatusCodes.Status400BadRequest,
                FormatException => StatusCodes.Status400BadRequest,
                ValidationException => StatusCodes.Status400BadRequest,
                BadHttpRequestException => StatusCodes.Status400BadRequest,

                // 401 - Não autenticado
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                SecurityException => StatusCodes.Status401Unauthorized,
                AuthenticationException => StatusCodes.Status401Unauthorized,

                // 403 - Sem permissão
                AccessViolationException => StatusCodes.Status403Forbidden,

                // 404 - Recurso não encontrado
                KeyNotFoundException => StatusCodes.Status404NotFound,
                FileNotFoundException => StatusCodes.Status404NotFound,
                DirectoryNotFoundException => StatusCodes.Status404NotFound,

                // 405 - Método não permitido
                NotSupportedException => StatusCodes.Status405MethodNotAllowed,

                // 408 - Timeout / cancelamento
                TaskCanceledException => StatusCodes.Status408RequestTimeout,
                TimeoutException => StatusCodes.Status408RequestTimeout,

                // 409 - Conflito (estado inválido, duplicidade, etc.)
                InvalidOperationException => StatusCodes.Status409Conflict,

                // 429 - Muitas requisições (throttling)
                HttpRequestException httpEx when httpEx.Message.Contains("429") => StatusCodes.Status429TooManyRequests,

                // 502 / 503 - Falhas de integração externa ou rede
                SocketException => StatusCodes.Status502BadGateway,
                HttpRequestException => StatusCodes.Status502BadGateway, // deve vir DEPOIS do with-filter acima
                IOException => StatusCodes.Status503ServiceUnavailable,
                OperationCanceledException => StatusCodes.Status503ServiceUnavailable,

                // 500 - Erro interno genérico (catch-all)
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
