using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using TweetService.DTOs;
using TweetService.Infrastructure.Attributes;
using TweetService.Interfaces.Services;

namespace TweetService.Infrastructure.Filters
{
    public class EnrichTweetsWithUserRelationshipActionFilter : IAsyncActionFilter
    {
        private readonly IUserTweetRelationshipService _userTweetRelationship;
        private readonly ILogger<EnrichTweetsWithUserRelationshipActionFilter> _logger;

        public EnrichTweetsWithUserRelationshipActionFilter(
            IUserTweetRelationshipService userTweetRelationship,
            ILogger<EnrichTweetsWithUserRelationshipActionFilter> logger)
        {
            _userTweetRelationship = userTweetRelationship;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var currentUserId = GetUserIdByAttribute(context);

            var resultContext = await next();

            if (currentUserId.HasValue && resultContext.Result is ObjectResult objectResult)
                objectResult.Value = await EnrichDataAsync(objectResult.Value, currentUserId.Value);
        }

        private Guid? GetUserIdByAttribute(ActionExecutingContext context)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
                return null;

            foreach (var parameter in actionDescriptor.MethodInfo.GetParameters())
            {
                var hasAttribute = parameter.GetCustomAttributes(typeof(CurrentUserIdAttribute), false).Length > 0;
                var name = parameter.Name;
                if (hasAttribute
                    && name != null
                    && context.ActionArguments.TryGetValue(name, out object? value))
                    return ParseUserId(value);
            }
            return null;
        }

        private Guid? ParseUserId(object? value)
        {
            if (value is Guid guid && guid != Guid.Empty)
                return guid;

            return null;
        }

        private async Task<object?> EnrichDataAsync(object? data, Guid userId)
        {
            switch (data)
            {
                case TweetDto tweetDto:
                    return await _userTweetRelationship.GetTweetWithRelationshipsAsync(tweetDto, userId);

                case IEnumerable<TweetDto> tweets:
                    return await _userTweetRelationship.GetTweetsWithRelationshipsAsync(tweets, userId);

                default:
                    _logger.LogWarning("Unsupported data type for enrichment: {Type}",
                        data?.GetType().Name);
                    return data;
            }
        }
    }
}