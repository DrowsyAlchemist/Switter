namespace FeedService.Interfaces.Data
{
    public interface IFollowsRepository
    {
        Task<IEnumerable<Guid>> GetFollowersAsync(Guid followingId);
        Task<IEnumerable<Guid>> GetFollowingsAsync(Guid followerId, int count);


        Task AddFollowerAsync(Guid followerId, Guid followingId);
        Task RemoveFollowerAsync(Guid followerId, Guid followingId);
    }
}
