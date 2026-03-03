using FeedService.DTOs;
using FeedService.Interfaces.Infrastructure;
using System.Text;
using System.Text.Json;

namespace FeedService.Services.Infrastructure
{
    public class TweetServiceClient : ITweetServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthTokenService _authTokenService;
        private readonly ILogger<TweetServiceClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public TweetServiceClient(HttpClient httpClient, IAuthTokenService authTokenService, ILogger<TweetServiceClient> logger)
        {
            _httpClient = httpClient;
            _authTokenService = authTokenService;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<TweetDto>> GetTweetsByIdAsync(List<Guid> tweetIds)
        {
            try
            {
                if (tweetIds.Any() == false)
                    return [];

                var queryBuilder = new StringBuilder("api/tweets/tweets?");

                foreach (var id in tweetIds)
                    queryBuilder.Append($"tweetIds={id}&");

                queryBuilder.Length--;

                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    queryBuilder.ToString()
                );

                var accessToken = _authTokenService.GetToken();
                if (accessToken != null)
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                
                var response = await GetResponse(request);
                var tweets = Deserialize<TweetDto>(response);
                return tweets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tweets.");
                return [];
            }
        }

        public async Task<List<Guid>> GetRecentUserTweetIdsAsync(Guid userId, int count)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/tweetQueries/user/tweets?userId={userId}&page={1}&pageSize={count}"
                );
                var response = await GetResponse(request);
                var tweetIds = Deserialize<Guid>(response);
                return tweetIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting RecentUserTweetIds for user {UserId}", userId);
                return [];
            }
        }

        public async Task<List<string>> GetTrendCategoriesAsync(int count)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/trend/categories?page={1}&pageSize={count}"
            );
            var response = await GetResponse(request);
            var categories = Deserialize<string>(response);
            return categories;
        }

        public async Task<List<Guid>> GetTrendTweetsIdsAsync(int count)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/trend/tweetIds?page={1}&pageSize={count}"
            );
            var response = await GetResponse(request);
            var trendTweetIds = Deserialize<Guid>(response);
            return trendTweetIds;
        }

        public async Task<List<Guid>> GetTrendTweetsIdsAsync(string hashtag, int count)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/trend/tweetIds/{hashtag}?page={1}&pageSize={count}"
            );
            var response = await GetResponse(request);
            var trendTweetIds = Deserialize<Guid>(response);
            return trendTweetIds;
        }

        private async Task<string> GetResponse(HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode == false)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get content from TweetService. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, errorContent);

                return string.Empty;
            }
            return await response.Content.ReadAsStringAsync();
        }

        private List<T> Deserialize<T>(string content)
        {
            var deserializedResponse = JsonSerializer.Deserialize<List<T>>(content, _jsonOptions);
            if (deserializedResponse == null)
            {
                _logger.LogError("Failed to deserialize content. Content: {content}", content);
                return [];
            }
            return deserializedResponse;
        }
    }
}
