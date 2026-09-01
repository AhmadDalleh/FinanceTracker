using System.Net;
using System.Text.Json;
using Application.Common.Exceptions;
using ValidationException = Application.Common.Exceptions.ValidationException;

namespace Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, extensions) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "One or more validation errors occurred.",
                new Dictionary<string, object?> { ["errors"] = validationException.Errors }),
            NotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, exception.Message, null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }

        var problemDetails = new Dictionary<string, object?>
        {
            ["title"] = title,
            ["status"] = (int)statusCode
        };

        if (extensions is not null)
        {
            foreach (var (key, value) in extensions)
            {
                problemDetails[key] = value;
            }
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
