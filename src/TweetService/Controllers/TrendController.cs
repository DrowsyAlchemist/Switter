using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TweetService.Infrastructure.Attributes;
using TweetService.Interfaces.Services;

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
        [ValidatePagination]
        public async Task<IActionResult> GetTrendCategoriesAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var trendCategories = await _trendService.GetTrendCategoriesAsync(page, pageSize);
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
        [ValidatePagination]
        public async Task<IActionResult> GetTrendTweetsAsync(
            [CurrentUserId] Guid? currentUserId,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var tweetDto = await _trendService.GetTrendTweetsAsync(currentUserId, page, pageSize);
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
        [ValidatePagination]
        public async Task<IActionResult> GetTrendTweetsAsync(
            string hashtag,
            [CurrentUserId] Guid? currentUserId, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrEmpty(hashtag))
                    return BadRequest("Hashtag shouldn't be null or empty.");

                var tweetDto = await _trendService.GetTrendTweetsAsync(hashtag, currentUserId, page, pageSize);
                _logger.LogInformation("Trend tweets by hashtag successfully sent.\nHashtag: {hashtag}", hashtag);
                return Ok(tweetDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetTrendTweets.\nHashtag: {hashtag}", hashtag);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}