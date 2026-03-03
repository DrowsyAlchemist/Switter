using Microsoft.AspNetCore.Mvc;
using TweetService.Infrastructure.Binders;

namespace TweetService.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class CurrentUserIdAttribute : ModelBinderAttribute
    {
        public CurrentUserIdAttribute() : base(typeof(CurrentUserIdModelBinder))
        {
        }
    }
}
