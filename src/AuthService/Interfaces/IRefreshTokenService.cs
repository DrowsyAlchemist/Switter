using AuthService.DTOs.Jwt;

namespace AuthService.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<RefreshTokenData> GenerateRefreshTokenAsync(Guid userId, string remoteIp);
        Task RevokeTokenAsync(string refreshToken, string remoteIp, string? replacedBy = null);
        Task<RefreshTokenData> RefreshAsync(string refreshToken, Guid userId, string remoteIp);
    }
}
