using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TweetService.Exceptions;
using TweetService.Infrastructure.Attributes;
using TweetService.Infrastructure.Filters;
using TweetService.Interfaces.Infrastructure;
using TweetService.Interfaces.Services;

namespace TweetService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TweetQueriesController : ControllerBase
    {
        private readonly ITweetQueries _tweetQueries;
        private readonly IUserServiceClient _userServiceClient;
        private readonly ILogger<TweetQueriesController> _logger;

        public TweetQueriesController(
            ITweetQueries tweetQueries,
            IUserServiceClient userServiceClient,
            ILogger<TweetQueriesController> logger)
        {
            _tweetQueries = tweetQueries;
            _userServiceClient = userServiceClient;
            _logger = logger;
        }

        [HttpGet("{tweetId}")]
        [ServiceFilter(typeof(EnrichTweetsWithUserRelationshipActionFilter))]
        public async Task<IActionResult> GetTweetAsync(Guid tweetId, [CurrentUserId] Guid? currentUserId)
        {
            try
            {
                var tweetDto = await _tweetQueries.GetTweetAsync(tweetId);
                _logger.LogInformation("Tweet successfully sent.\nId:{tweetId}", tweetId);
                return Ok(tweetDto);
            }
            catch (TweetNotFoundException ex)
            {
                _logger.LogWarning(ex, "GetTweet failed. Tweet not found.\nId: {tweetId}", tweetId);
                return NotFound("Tweet not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetTweet.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("tweets")]
        [ServiceFilter(typeof(EnrichTweetsWithUserRelationshipActionFilter))]
        public async Task<IActionResult> GetTweetsAsync(List<Guid> tweetIds, [CurrentUserId] Guid? currentUserId)
        {
            try
            {
                var tweetDto = await _tweetQueries.GetTweetsAsync(tweetIds);
                _logger.LogInformation("Tweets successfully sent.");
                return Ok(tweetDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetTweets.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("user/{userId}")]
        [ValidatePagination]
        [ServiceFilter(typeof(EnrichTweetsWithUserRelationshipActionFilter))]
        public async Task<IActionResult> GetUserTweetsAsync(
            Guid userId,
            [CurrentUserId] Guid? currentUserId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var tweetDtos = await _tweetQueries.GetUserTweetsAsync(userId, page, pageSize);
                _logger.LogInformation("User's tweets successfully sent.\nUserId:{userId}", userId);
                return Ok(tweetDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetUserTweets.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("user/tweets")]
        [ValidatePagination]
        public async Task<IActionResult> GetUserTweetIdsAsync(
            Guid userId,
            [CurrentUserId] Guid? currentUserId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var tweetIds = await _tweetQueries.GetUserTweetIdsAsync(userId, page, pageSize);
                _logger.LogInformation("User's tweets successfully sent.\nUserId:{userId}", userId);
                return Ok(tweetIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetUserTweetIds.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("me")]
        [ValidatePagination]
        [ServiceFilter(typeof(EnrichTweetsWithUserRelationshipActionFilter))]
        public async Task<IActionResult> GetMyTweets(
            [CurrentUserId] Guid? currentUserId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (currentUserId.HasValue == false)
            {
                _logger.LogWarning("GetMyTweets failed. Current user not found.");
                return Unauthorized();
            }
            try
            {
                var tweetDtos = await _tweetQueries.GetUserTweetsAsync(currentUserId.Value, page, pageSize);
                _logger.LogInformation("User's tweets successfully sent.\nUserId:{userId}", currentUserId.Value);
                return Ok(tweetDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetMyTweets.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }


        [HttpGet("replies/{tweetId}")]
        [ValidatePagination]
        [ServiceFilter(typeof(EnrichTweetsWithUserRelationshipActionFilter))]
        public async Task<IActionResult> GetTweetRepliesAsync(
            Guid tweetId,
            [CurrentUserId] Guid? currentUserId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var tweetDtos = await _tweetQueries.GetTweetRepliesAsync(tweetId, page, pageSize);
                _logger.LogInformation("Tweet replies successfully sent.\nTweetId:{tweetId}", tweetId);
                return Ok(tweetDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetTweetReplies.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
