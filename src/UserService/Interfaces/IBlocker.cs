namespace UserService.Interfaces
{
    public interface IBlocker
    {
        Task BlockUserAsync(Guid blocker, Guid blocked);
    }
}
