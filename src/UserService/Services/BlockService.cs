using AutoMapper;
using UserService.Exceptions.Follows;
using UserService.Interfaces.Data;
using UserService.Interfaces;
using UserService.DTOs;

namespace UserService.Services
{
    public class BlockService : IBlockService, IBlockChecker
    {
        private readonly IBlockRepository _blockRepository;
        private readonly IMapper _mapper;

        public BlockService(IBlockRepository blockRepository, IMapper mapper)
        {
            _blockRepository = blockRepository;
            _mapper = mapper;
        }

        public async Task BlockAsync(Guid blockerId, Guid blockedId)
        {
            if (blockerId == blockedId)
                throw new SelfFollowException();

            bool isBlocked = await _blockRepository.IsBlockedAsync(blockerId, blockedId);
            if (isBlocked)
                throw new DoubleFollowException();

            await _blockRepository.AddAsync(blockerId, blockedId);
        }

        public async Task UnblockAsync(Guid blockerId, Guid blockedId)
        {
            bool isBlocked = await _blockRepository.IsBlockedAsync(blockerId, blockedId);
            if (isBlocked == false)
                throw new FollowNotFoundException(blockerId, blockedId);

            await _blockRepository.DeleteAsync(blockerId, blockedId);
        }

        public async Task<List<UserProfileDto>> GetBlockedAsync(Guid blockerId, int page = 1, int pageSize = 20)
        {
            var blockedUsers = await _blockRepository.GetBlockedAsync(blockerId);
            blockedUsers = blockedUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return _mapper.Map<List<UserProfileDto>>(blockedUsers);
        }

        public async Task<bool> IsBlocked(Guid blockerId, Guid blockedId)
        {
            return await _blockRepository.IsBlockedAsync(blockerId, blockedId);
        }
    }
}
