using AutoMapper;
using UserService.Interfaces.Data;
using UserService.Interfaces;
using UserService.DTOs;
using UserService.Exceptions.Blocks;

namespace UserService.Services
{
    public class BlockService : IBlockService, IBlockChecker
    {
        private readonly IBlockRepository _blockRepository;
        private readonly Blocker _blocker;
        private readonly IMapper _mapper;

        public BlockService(IBlockRepository blockRepository, Blocker blocker, IMapper mapper)
        {
            _blockRepository = blockRepository;
            _blocker = blocker;
            _mapper = mapper;
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

        public async Task<List<UserProfileDto>> GetBlockedAsync(Guid blockerId, int page = 1, int pageSize = 20)
        {
            var blockedUsers = await _blockRepository.GetBlockedAsync(blockerId);
            blockedUsers = blockedUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return _mapper.Map<List<UserProfileDto>>(blockedUsers);
        }

        public async Task<bool> IsBlockedAsync(Guid blockerId, Guid blockedId)
        {
            return await _blockRepository.IsBlockedAsync(blockerId, blockedId);
        }
    }
}
