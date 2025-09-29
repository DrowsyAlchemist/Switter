using AuthService.DTOs;

namespace AuthService.Interfaces
{
    interface IAuthService
    {
        Task<LoginResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task RevokeTokenAsync(string refreshToken, string ipAddress);
        Task<bool> ValidateTokenAsync(string token);

    }
}
