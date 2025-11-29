using AuthService.Data;
using AuthService.DTOs.Auth;
using AuthService.Interfaces.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AuthService.HealthChecks
{
    public class AuthHealthCheck : IHealthCheck
    {
        private readonly ILogger<AuthHealthCheck> _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly AuthDbContext _authDbContext;

        public AuthHealthCheck(ILogger<AuthHealthCheck> logger, IAuthorizationService authorizationService, AuthDbContext authDbContext)
        {
            _logger = logger;
            _authorizationService = authorizationService;
            _authDbContext = authDbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var registerRequest = new RegisterRequest()
                {
                    Username = "TestUser",
                    Email = "test@email.ru",
                    Password = "SecurePassword123!",
                    ConfirmPassword = "SecurePassword123"
                };
                var registerResponse = await _authorizationService.RegisterAsync(registerRequest, "0.0.0.0");

                var loginRequest = new LoginRequest()
                {
                    Login = registerRequest.Email,
                    Password = registerRequest.Password,
                };
                var loginResponse = await _authorizationService.LoginAsync(loginRequest, "0.0.0.0");

                var refreshRequest = new RefreshRequest()
                {
                    AccessToken = loginResponse.AccessToken,
                    RefreshToken = loginResponse.RefreshToken
                };
                var refreshResponse = await _authorizationService.RefreshTokenAsync(refreshRequest, "0.0.0.0");

                await _authorizationService.RevokeTokenAsync(refreshResponse.RefreshToken, "0.0.0.0");
                var user = await _authDbContext.Users.Where(u => u.Email == registerRequest.Email).FirstOrDefaultAsync();
                _authDbContext.Users.Remove(user!);
                await _authDbContext.SaveChangesAsync();
                return HealthCheckResult.Healthy("Auth service is working");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auth service health check failed");
                return HealthCheckResult.Unhealthy("Auth service exception");
            }
        }
    }
}
