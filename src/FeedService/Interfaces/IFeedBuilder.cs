namespace FeedService.Interfaces
{
    public interface IFeedBuilder
    {
        Task BuildFeedAsync(Guid userId);
    }
}
