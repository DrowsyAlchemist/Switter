using FeedService.Interfaces.Infrastructure;

namespace FeedService.Services.Infrastructure
{
    public class AuthTokenService : IAuthTokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthTokenService> _logger;

        public AuthTokenService(IHttpContextAccessor httpContextAccessor, ILogger<AuthTokenService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string? GetToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogDebug("HttpContext is null");
                return null;
            }

            var authHeader = httpContext.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return authHeader.Substring("Bearer ".Length).Trim();

            if (httpContext.User?.Identity?.IsAuthenticated ?? false)
                _logger.LogDebug("User is authenticated but no token found in headers or claims");
            else
                _logger.LogDebug("User is not authenticated");

            return null;
        }
    }
}
