using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TweetService.Attributes;
using TweetService.Exceptions;
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
        public async Task<IActionResult> GetLikedTweetsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var tweetDtos = await _likeService.GetLikedTweetsAsync(currentUserId, page, pageSize);
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
        public async Task<IActionResult> LikeTweetAsync(Guid tweetId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                await _likeService.LikeTweetAsync(tweetId, currentUserId);
                _logger.LogInformation("Successfully liked.\nTweetId: {tweetId}.", tweetId);
                return Ok(new { message = "Tweet successfully liked." });
            }
            catch (DoubleLikeException ex)
            {
                _logger.LogWarning(ex, "Like tweet failed.\nTweet is already liked.");
                return BadRequest("Tweet is already liked.");
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
        public async Task<IActionResult> UnlikeTweetAsync(Guid tweetId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                await _likeService.UnlikeTweetAsync(tweetId, currentUserId);
                _logger.LogInformation("Successfully unliked.\nTweetId: {tweetId}.", tweetId);
                return Ok(new { message = "Tweet successfully unliked." });
            }
            catch (LikeNotFoundException ex)
            {
                _logger.LogWarning(ex, "Unlike tweet failed.\nTweet is not liked.");
                return BadRequest("Tweet is not liked.");
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

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
                return Guid.Parse(userId!);

            throw new Exception("Current user not found.");
        }
    }
}
