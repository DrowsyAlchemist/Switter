using AutoMapper;
using UserService.DTOs;
using UserService.Interfaces.Data;
using UserService.Interfaces.Queries;

namespace UserService.Services.Queries
{
    public class BlockQueries : IBlockQueries
    {
        private readonly IBlockRepository _blockRepository;
        private readonly IMapper _mapper;

        public BlockQueries(IBlockRepository blockRepository, IMapper mapper)
        {
            _blockRepository = blockRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserProfileDto>> GetBlockedAsync(Guid blockerId, int page, int pageSize)
        {
            var blockedUsers = await _blockRepository.GetBlockedAsync(blockerId, page, pageSize);
            return _mapper.Map<List<UserProfileDto>>(blockedUsers);
        }

        public async Task<IEnumerable<Guid>> GetBlockedIdsAsync(Guid blockerId, int page, int pageSize)
        {
            return await _blockRepository.GetBlockedIdsAsync(blockerId, page, pageSize);
        }

        public async Task<bool> IsBlockedAsync(Guid blockerId, Guid blockedId)
        {
            return await _blockRepository.IsBlockedAsync(blockerId, blockedId);
        }
    }
}
