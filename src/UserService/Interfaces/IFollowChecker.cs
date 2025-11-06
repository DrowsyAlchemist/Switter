namespace UserService.Interfaces
{
    public interface IFollowChecker
    {
        Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId);
    }
}
