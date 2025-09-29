using AuthService.Data;
using AuthService.DTOs;
using AuthService.Interfaces;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AuthDbContext context, IJwtService jwtService,
                          IUserService userService, ILogger<AuthService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _userService.UserExistsAsync(request.Email, request.Username))
                throw new ArgumentException("User with this email or username already exists");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered: {Username} ({Email})", user.Username, user.Email);

            return await GenerateAuthResponseAsync(user, request.IpAddress);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userService.GetUserByEmailAsync(request.Login)
                  ?? await _userService.GetUserByUsernameAsync(request.Login);

            if (user == null)
                throw new Exception($"User with login {request.Login} in not found");

            if (BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash) == false)
                throw new UnauthorizedAccessException("Invalid password");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated");

            _logger.LogInformation("User logged in: {Username}", user.Username);
            return await GenerateAuthResponseAsync(user, request.IpAddress);
        }

        public Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            throw new NotImplementedException();
        }

        public Task RevokeTokenAsync(string refreshToken, string ipAddress)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            throw new NotImplementedException();
        }

        private async Task<AuthResponse> GenerateAuthResponseAsync(User user, string ipAddress)
        {
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken.Token,
                Expires = refreshToken.Expires,
                CreatedByIp = ipAddress,
                UserId = user.Id
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                AccessToken = accessToken.Token,
                RefreshToken = refreshToken.Token,
                ExpiresAt = accessToken.Expires
            };
        }

    }
}
