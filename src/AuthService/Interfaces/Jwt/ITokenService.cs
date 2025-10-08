using AuthService.DTOs.Jwt;
namespace AuthService.Interfaces.Jwt
{
    public interface ITokenService
    {
        AccessTokenData GenerateAccessToken(UserClaims user);
        ValidateTokenResult ValidateAccessToken(string token);
        Task<RefreshTokenData> GenerateRefreshTokenAsync(Guid userId, string remoteIp);
        Task RevokeTokenAsync(string refreshToken, string remoteIp, string? replacedBy = null);
        Task<RefreshTokenData> RefreshAsync(string refreshToken, Guid userId, string remoteIp);
    }
}
