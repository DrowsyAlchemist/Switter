using AuthService.DTOs.Jwt;
using AuthService.Interfaces.Jwt;

namespace AuthService.Services.Jwt
{
    internal class TokenService : ITokenService
    {
        private readonly IAccessTokenService _accessTokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public TokenService(IAccessTokenService accessTokenService, IRefreshTokenService refreshTokenService)
        {
            _accessTokenService = accessTokenService;
            _refreshTokenService = refreshTokenService;
        }

        public AccessTokenData GenerateAccessToken(UserClaims user)
        {
            return _accessTokenService.GenerateAccessToken(user);
        }

        public async Task<RefreshTokenData> GenerateRefreshTokenAsync(Guid userId, string remoteIp)
        {
            return await _refreshTokenService.GenerateRefreshTokenAsync(userId, remoteIp);
        }

        public async Task<RefreshTokenData> RefreshAsync(string refreshToken, Guid userId, string remoteIp)
        {
            return await _refreshTokenService.RefreshAsync(refreshToken, userId, remoteIp);
        }

        public async Task RevokeTokenAsync(string refreshToken, string remoteIp, string? replacedBy = null)
        {
            await _refreshTokenService.RevokeTokenAsync(refreshToken, remoteIp, replacedBy);
        }

        public Guid? ValidateAccessToken(string token)
        {
            return _accessTokenService.ValidateAccessToken(token);
        }
    }
}
