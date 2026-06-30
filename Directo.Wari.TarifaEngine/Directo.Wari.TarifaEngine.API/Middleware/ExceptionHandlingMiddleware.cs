using System.Net;
using System.Text.Json;

namespace Directo.Wari.TarifaEngine.API.Middleware
{
    /// <summary>
    /// Middleware global para manejo de excepciones no controladas.
    /// </summary>
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
                _logger.LogError(ex, "Excepción no controlada: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                Domain.Exceptions.EntityNotFoundException => (HttpStatusCode.NotFound, exception.Message),
                Domain.Exceptions.BusinessRuleException => (HttpStatusCode.BadRequest, exception.Message),
                Domain.Exceptions.InvalidStateTransitionException => (HttpStatusCode.Conflict, exception.Message),
                Application.Common.Exceptions.ValidationException validationEx => (HttpStatusCode.BadRequest, JsonSerializer.Serialize(validationEx.Errors)),
                Application.Common.Exceptions.ForbiddenAccessException => (HttpStatusCode.Forbidden, exception.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "No autorizado."),
                _ => (HttpStatusCode.InternalServerError, "Ha ocurrido un error interno del servidor.")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                status = (int)statusCode,
                error = message,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}
