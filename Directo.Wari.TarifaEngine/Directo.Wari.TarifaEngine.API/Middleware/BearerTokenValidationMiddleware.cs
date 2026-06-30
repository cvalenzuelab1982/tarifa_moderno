using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Directo.Wari.TarifaEngine.API.Middleware
{
    public class BearerTokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BearerTokenValidationMiddleware> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _validationUrl;
        private readonly bool _validationEnabled;

        private static readonly string[] SkippedPrefixes =
        [
            "/health",
            "/swagger",
            "/hubs/"
        ];

        public BearerTokenValidationMiddleware(RequestDelegate next, ILogger<BearerTokenValidationMiddleware> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _validationEnabled = configuration.GetValue<bool>("TokenValidation:Enabled", true);
            _validationUrl = configuration["TokenValidation:Url"] ?? "https://localhost:5001/Auth/postValidarToken";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Permitir solicitudes preflight CORS sin validación
            if (HttpMethods.IsOptions(context.Request.Method))
            {
                await _next(context);
                return;
            }

            if (!_validationEnabled)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value ?? string.Empty;

            if (IsPublicPath(path))
            {
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                await WriteUnauthorizedAsync(context, "Token de acceso requerido.");
                return;
            }

            var token = authHeader["Bearer ".Length..].Trim();

            if (!await ValidateTokenAsync(token, context.RequestAborted))
            {
                await WriteUnauthorizedAsync(context, "Token inválido o expirado.");
                return;
            }

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var claims = jwt.Claims.ToList();

            var identity = new ClaimsIdentity(claims, "Bearer");
            context.User = new ClaimsPrincipal(identity);

            await _next(context);
        }

        private static bool IsPublicPath(string path)
        {
            foreach (var prefix in SkippedPrefixes)
            {
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // El endpoint de login es siempre público
            return path.Contains("/auth/login", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TokenValidation");
                var response = await client.PostAsJsonAsync(
                    _validationUrl,
                    new { token },
                    cancellationToken);

                var json = await response.Content.ReadAsStringAsync();

                _logger.LogInformation(
                    "Respuesta de validación: {Json}",
                    json);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("El servicio de validación retornó HTTP {StatusCode}.", (int)response.StatusCode);
                    return false;
                }

                var result = await response.Content
                    .ReadFromJsonAsync<TokenValidationResponse>(cancellationToken: cancellationToken);

                if (result is null)
                {
                    _logger.LogWarning("El servicio de validación retornó una respuesta vacía o no deserializable.");
                    return false;
                }

                if (result.Resultado < 0)
                    _logger.LogWarning("Token rechazado por el servicio de validación: {Message}", result.Valor);

                return result.Resultado == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contactar el servicio de validación de tokens en {Url}.", _validationUrl);
                return false;
            }
        }

        private static async Task WriteUnauthorizedAsync(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = 401,
                error = message,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }

        private sealed record TokenValidationResponse(int Resultado, string? Valor);
    }
}
