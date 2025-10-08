using AuthService.DTOs.Jwt;
using AuthService.Interfaces.Jwt;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AuthService.HealthChecks
{
    public class TokenServiceHealthCheck : IHealthCheck
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<TokenServiceHealthCheck> _logger;

        public TokenServiceHealthCheck(ITokenService tokenService, ILogger<TokenServiceHealthCheck> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var testPayload = new UserClaims { Id = Guid.NewGuid(), Name = "health-check", Email = "health@test.com" };
                var testIp = "0.0.0.0";
                var accessTokenData = _tokenService.GenerateAccessToken(testPayload);

                var resultUserId = _tokenService.ValidateAccessToken(accessTokenData.Token);

                if (resultUserId != testPayload.Id)
                    throw new Exception("Id after access token validation does not match with test user id");

                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(testPayload.Id, testIp);
                var newRefreshToken = await _tokenService.RefreshAsync(refreshToken.Token, testPayload.Id, testIp);
                await _tokenService.RevokeTokenAsync(newRefreshToken.Token, testIp);

                return HealthCheckResult.Healthy("Token service is working");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token service health check failed");
                return HealthCheckResult.Unhealthy("Token service exception");
            }
        }
    }
}
