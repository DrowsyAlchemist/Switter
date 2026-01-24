using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace TweetService.Infrastructure.Binders
{
    public class CurrentUserIdModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(Guid?))
                return Task.CompletedTask;

            var userIdClaim = bindingContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                bindingContext.ModelState.TryAddModelError(
                    bindingContext.ModelName,
                    $"Invalid User ID format: {userIdClaim}");
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(userId);
            return Task.CompletedTask;
        }
    }
}