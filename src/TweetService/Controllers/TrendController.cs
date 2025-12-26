using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TweetService.Exceptions;
using TweetService.Interfaces.Services;
using TweetService.Models;
using TweetService.Services;

namespace TweetService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrendController : ControllerBase
    {
        private readonly ITrendService _trendService;
        private readonly ILogger<TrendController> _logger;

        public TrendController(ITrendService trendService, ILogger<TrendController> logger)
        {
            _trendService = trendService;
            _logger = logger;
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetTrendCategoriesAsync()
        {
            try
            {
                var trendCategories = await _trendService.GetTrendCategoriesAsync();
                _logger.LogInformation("Trend categories successfully sent.");
                return Ok(trendCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetTrendCategories.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("tweets")]
        public async Task<IActionResult> GetTrendTweetsAsync()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var tweetDto = await _trendService.GetTrendTweetsAsync(currentUserId);
                _logger.LogInformation("Trend tweets successfully sent.");
                return Ok(tweetDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetTrendTweets.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("tweets/{hashtag}")]
        public async Task<IActionResult> GetTrendTweetsAsync(string hashtag)
        {
            try
            {
                if (string.IsNullOrEmpty(hashtag))
                    return BadRequest("Hashtag shouldn't be null or empty.");

                var currentUserId = GetCurrentUserId();
                var tweetDto = await _trendService.GetTrendTweetsAsync(hashtag, currentUserId);
                _logger.LogInformation("Trend tweets by hashtag successfully sent.\nHashtag: {hashtag}", hashtag);
                return Ok(tweetDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetTrendTweets.\nHashtag: {hashtag}", hashtag);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
                return Guid.Parse(userId!);

            return null;
        }
    }
}
