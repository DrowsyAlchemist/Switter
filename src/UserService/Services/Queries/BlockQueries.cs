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
