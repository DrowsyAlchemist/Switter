using UserService.Exceptions.Blocks;
using UserService.Interfaces;
using UserService.Interfaces.Commands;
using UserService.Interfaces.Data;

namespace UserService.Services.Commands
{
    public class BlockCommands : IBlockCommands
    {
        private readonly IBlockRepository _blockRepository;
        private readonly IBlocker _blocker;

        public BlockCommands(IBlockRepository blockRepository, IBlocker blocker)
        {
            _blockRepository = blockRepository;
            _blocker = blocker;
        }

        public async Task BlockAsync(Guid blockerId, Guid blockedId)
        {
            if (blockerId == blockedId)
                throw new SelfBlockException();

            bool isBlocked = await _blockRepository.IsBlockedAsync(blockerId, blockedId);
            if (isBlocked)
                throw new DoubleBlockException();

            await _blocker.BlockUserAsync(blockerId, blockedId);
        }

        public async Task UnblockAsync(Guid blockerId, Guid blockedId)
        {
            bool isBlocked = await _blockRepository.IsBlockedAsync(blockerId, blockedId);
            if (isBlocked == false)
                throw new BlockNotFoundException(blockerId, blockedId);

            await _blockRepository.DeleteAsync(blockerId, blockedId);
        }
    }
}
