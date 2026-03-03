using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TweetService.Exceptions;
using TweetService.Infrastructure.Attributes;
using TweetService.Infrastructure.Filters;
using TweetService.Interfaces.Services;

namespace TweetService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;
        private readonly ILogger<LikeController> _logger;

        public LikeController(ILikeService likeService, ILogger<LikeController> logger)
        {
            _likeService = likeService;
            _logger = logger;
        }

        [HttpGet("liked")]
        [ValidatePagination]
        [ServiceFilter(typeof(EnrichTweetsWithUserRelationshipActionFilter))]
        public async Task<IActionResult> GetLikedTweetsAsync(
            [CurrentUserId] Guid? currentUserId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (currentUserId.HasValue == false)
            {
                _logger.LogWarning("GetLikedTweets failed. Current user not found.");
                return Unauthorized();
            }
            try
            {
                var tweetDtos = await _likeService.GetLikedTweetsAsync(currentUserId.Value, page, pageSize);
                _logger.LogInformation("Liked tweets successfully sent.");
                return Ok(tweetDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetLikedTweets.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LikeTweetAsync(Guid tweetId, [CurrentUserId] Guid? currentUserId)
        {
            if (currentUserId.HasValue == false)
            {
                _logger.LogWarning("LikeTweet failed. Current user not found.");
                return Unauthorized();
            }
            try
            {
                await _likeService.LikeTweetAsync(tweetId, currentUserId.Value);
                _logger.LogInformation("Successfully liked.\nTweetId: {tweetId}.", tweetId);
                return Ok(new { message = "Tweet successfully liked." });
            }
            catch (DoubleLikeException ex)
            {
                _logger.LogWarning(ex, "Like tweet failed.\nTweet is already liked.");
                return Ok("Tweet is already liked.");
            }
            catch (TweetNotFoundException ex)
            {
                _logger.LogWarning(ex, "Like tweet failed.\nTweet not found.\nTweetId: {tweetId}", tweetId);
                return NotFound("Tweet not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during LikeTweet.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> UnlikeTweetAsync(Guid tweetId, [CurrentUserId] Guid? currentUserId)
        {
            if (currentUserId.HasValue == false)
            {
                _logger.LogWarning("UnlikeTweet failed. Current user not found.");
                return Unauthorized();
            }
            try
            {
                await _likeService.UnlikeTweetAsync(tweetId, currentUserId.Value);
                _logger.LogInformation("Successfully unliked.\nTweetId: {tweetId}.", tweetId);
                return Ok(new { message = "Tweet successfully unliked." });
            }
            catch (LikeNotFoundException ex)
            {
                _logger.LogWarning(ex, "Unlike tweet failed.\nTweet is not liked.");
                return Ok("Tweet is not liked.");
            }
            catch (TweetNotFoundException ex)
            {
                _logger.LogWarning(ex, "Unlike tweet failed.\nTweet not found.\nTweetId: {tweetId}", tweetId);
                return NotFound("Tweet not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during UnlikeTweet.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
