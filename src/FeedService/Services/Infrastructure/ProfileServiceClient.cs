using FeedService.Interfaces.Infrastructure;
using System.Text.Json;

namespace FeedService.Services.Infrastructure
{
    public class ProfileServiceClient : IProfileServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthTokenService _authTokenService;
        private readonly ILogger<ProfileServiceClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProfileServiceClient(HttpClient httpClient, IAuthTokenService authTokenService, ILogger<ProfileServiceClient> logger)
        {
            _httpClient = httpClient;
            _authTokenService = authTokenService;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<Guid>> GetFollowersAsync(Guid userId)
        {
            try
            {
                var requestUri = $"api/follow/followerIds/{userId}?page={1}&pageSize={int.MaxValue}";
                return await GetIds(requestUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting follower ids for user {UserId}", userId);
                return [];
            }
        }
        public async Task<IEnumerable<Guid>> GetFollowingsAsync(Guid userId, int count)
        {
            try
            {
                var requestUri = $"api/follow/followingIds/{userId}?page={1}&pageSize={count}";
                return await GetIds(requestUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting following ids for user {UserId}", userId);
                return [];
            }
        }

        public async Task<IEnumerable<Guid>> GetBlocked(Guid blockerId)
        {
            try
            {
                var requestUri = $"api/follow/getBlockedIds/{blockerId}?page={1}&pageSize={int.MaxValue}";
                return await GetIds(requestUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blocked user ids for user {UserId}", blockerId);
                return [];
            }
        }

        private async Task<IEnumerable<Guid>> GetIds(string requestUri)
        {
            var accessToken = _authTokenService.GetToken();
            if (string.IsNullOrEmpty(accessToken))
                return [];

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                requestUri
            );
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return new List<Guid>();

            if (response.IsSuccessStatusCode == false)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get user ids. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, errorContent);

                return [];
            }
            var content = await response.Content.ReadAsStringAsync();
            var deserializedResponce = JsonSerializer.Deserialize<IEnumerable<Guid>>(content, _jsonOptions);
            if (deserializedResponce == null)
            {
                _logger.LogError("Failed to deserialize content. Content: {content}", content);
                return [];
            }
            return deserializedResponce;
        }
    }
}
