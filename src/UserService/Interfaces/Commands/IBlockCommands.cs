namespace UserService.Interfaces.Commands
{
    public interface IBlockCommands
    {
        Task BlockAsync(Guid blocker, Guid blocked);
        Task UnblockAsync(Guid blocker, Guid blocked);
    }
}
