using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using System.Security.Claims;

namespace Directo.Wari.TarifaEngine.API.Extensions
{
    /// <summary>
    /// Implementación del servicio de usuario actual basado en HttpContext.
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst("id")?.Value;
        public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirst("username")?.Value;
        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
