using CloudInvoice.Identity.Api.Middlewares.Exceptions;
using System.Text.Json;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // Deixa o pedido avançar na pipeline
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message); // Regista o erro no servidor
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            statusCode = 0,
            message = "",
            details = "",
            errors = (Dictionary<string, string[]>)null
        };

        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = validationEx.StatusCode;
                response = new
                {
                    statusCode = validationEx.StatusCode,
                    message = validationEx.Message,
                    details = exception.InnerException?.Message ?? "",
                    errors = validationEx.Errors
                };
                break;

            case NotFoundException notFoundEx:
                context.Response.StatusCode = notFoundEx.StatusCode;
                response = new
                {
                    statusCode = notFoundEx.StatusCode,
                    message = notFoundEx.Message,
                    details = exception.InnerException?.Message ?? "",
                    errors = (Dictionary<string, string[]>)null
                };
                break;

            case ConflictException conflictEx:
                context.Response.StatusCode = conflictEx.StatusCode;
                response = new
                {
                    statusCode = conflictEx.StatusCode,
                    message = conflictEx.Message,
                    details = exception.InnerException?.Message ?? "",
                    errors = (Dictionary<string, string[]>)null
                };
                break;

            case UnauthorizedException unauthorizedEx:
                context.Response.StatusCode = unauthorizedEx.StatusCode;
                response = new
                {
                    statusCode = unauthorizedEx.StatusCode,
                    message = unauthorizedEx.Message,
                    details = exception.InnerException?.Message ?? "",
                    errors = (Dictionary<string, string[]>)null
                };
                break;

            case ForbiddenException forbiddenEx:
                context.Response.StatusCode = forbiddenEx.StatusCode;
                response = new
                {
                    statusCode = forbiddenEx.StatusCode,
                    message = forbiddenEx.Message,
                    details = exception.InnerException?.Message ?? "",
                    errors = (Dictionary<string, string[]>)null
                };
                break;

            case DatabaseException databaseEx:
                context.Response.StatusCode = databaseEx.StatusCode;
                response = new
                {
                    statusCode = databaseEx.StatusCode,
                    message = databaseEx.Message,
                    details = exception.InnerException?.Message ?? "",
                    errors = (Dictionary<string, string[]>)null
                };
                break;

            case AppException appEx:
                context.Response.StatusCode = appEx.StatusCode;
                response = new
                {
                    statusCode = appEx.StatusCode,
                    message = appEx.Message,
                    details = exception.InnerException?.Message ?? "",
                    errors = (Dictionary<string, string[]>)null
                };
                break;

            default:
                // Erro genérico/não esperado
                context.Response.StatusCode = 500;
                response = new
                {
                    statusCode = 500,
                    message = "Ocorreu um erro interno no servidor.",
                    details = exception.Message,
                    errors = (Dictionary<string, string[]>)null
                };
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(jsonResponse);
    }
}