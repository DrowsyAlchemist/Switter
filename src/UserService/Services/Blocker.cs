using UserService.Data;
using UserService.Exceptions.Follows;
using UserService.Interfaces;
using UserService.Interfaces.Commands;
using UserService.Interfaces.Data;

namespace UserService.Services
{
    public class Blocker : IBlocker
    {
        private readonly UserDbContext _dbContext;
        private readonly IBlockRepository _blockRepository;
        private readonly IFollowCommands _followCommands;

        public Blocker(UserDbContext context, IBlockRepository blockRepository, IFollowCommands followCommands)
        {
            _dbContext = context;
            _blockRepository = blockRepository;
            _followCommands = followCommands;
        }

        public async Task BlockUserAsync(Guid blocker, Guid blocked)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _blockRepository.AddAsync(blocker, blocked);
                await UnfollowIfFollowingExistsAsync(blocker, blocked);
                await UnfollowIfFollowingExistsAsync(blocked, blocker);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task UnfollowIfFollowingExistsAsync(Guid followerId, Guid followeeId)
        {
            try
            {
                await _followCommands.UnfollowUserAsync(followerId, followeeId);
            }
            catch (FollowNotFoundException) { return; }
        }
    }
}