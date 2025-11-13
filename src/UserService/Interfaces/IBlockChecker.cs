namespace UserService.Interfaces
{
    public interface IBlockChecker
    {
        Task<bool> IsBlocked(Guid blockerId, Guid blockedId);
    }
}
