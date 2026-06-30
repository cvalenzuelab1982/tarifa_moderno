namespace Directo.Wari.TarifaEngine.API.Middleware
{
    /// <summary>
    /// Middleware que agrega un Correlation ID a cada request para trazabilidad.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-Id";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Request.Headers[CorrelationIdHeader] = Guid.NewGuid().ToString();
            }

            var correlationId = context.Request.Headers[CorrelationIdHeader].ToString();
            context.Response.Headers[CorrelationIdHeader] = correlationId;

            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
