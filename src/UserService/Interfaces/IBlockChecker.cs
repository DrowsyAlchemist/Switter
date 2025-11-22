namespace UserService.Interfaces
{
    public interface IBlockChecker
    {
        Task<bool> IsBlockedAsync(Guid blockerId, Guid blockedId);
    }
}
