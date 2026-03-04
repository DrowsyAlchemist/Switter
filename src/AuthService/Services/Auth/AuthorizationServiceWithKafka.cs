using AuthService.DTOs.Auth;
using AuthService.Interfaces.Auth;
using AuthService.Interfaces.Infrastructure;
using AuthService.Models.Options;
using Microsoft.Extensions.Options;
using UserService.KafkaEvents.AuthEvents;

namespace AuthService.Services.Auth
{
    internal class AuthorizationServiceWithKafka : IAuthorizationService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IKafkaProducerService _kafkaProducerService;
        private readonly KafkaOptions _kafkaOptions;

        public AuthorizationServiceWithKafka(
            IAuthorizationService authorizationService,
            IKafkaProducerService kafkaProducerService,
            IOptions<KafkaOptions> kafkaOptions)
        {
            _authorizationService = authorizationService;
            _kafkaProducerService = kafkaProducerService;
            _kafkaOptions = kafkaOptions.Value;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string remoteIp)
        {
            var response = await _authorizationService.RegisterAsync(request, remoteIp);

            var registeredEvent = new UserRegisteredEvent(response.UserId, response.Username, response.Email, DateTime.UtcNow);
            string eventName = _kafkaOptions.AuthEvents.UserRegisteredEventName;
            await _kafkaProducerService.ProduceAsync(eventName, registeredEvent);

            return response;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, string remoteIp)
        {
           return await _authorizationService.LoginAsync(request, remoteIp);
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshRequest request, string remoteIp)
        {
           return await _authorizationService.RefreshTokenAsync(request, remoteIp);
        }

        public async Task RevokeTokenAsync(string refreshToken, string remoteIp)
        {
            await _authorizationService.RevokeTokenAsync(refreshToken, remoteIp);
        }
    }
}