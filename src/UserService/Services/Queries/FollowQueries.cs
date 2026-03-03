using AutoMapper;
using UserService.Interfaces.Data;
using UserService.Interfaces.Queries;
using UserService.DTOs;

namespace UserService.Services.Queries
{
    public class FollowQueries : IFollowQueries
    {
        private readonly IFollowRepository _followRepository;
        private readonly IMapper _mapper;

        public FollowQueries(IFollowRepository followRepository, IMapper mapper)
        {
            _followRepository = followRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserProfileDto>> GetFollowersAsync(Guid userId, int page, int pageSize)
        {
            var followers = await _followRepository.GetFollowersAsync(userId, page, pageSize);
            return _mapper.Map<List<UserProfileDto>>(followers);
        }

        public async Task<IEnumerable<UserProfileDto>> GetFollowingsAsync(Guid userId, int page, int pageSize)
        {
            var followings = await _followRepository.GetFollowingsAsync(userId, page, pageSize);
            return _mapper.Map<List<UserProfileDto>>(followings);
        }

        public async Task<IEnumerable<Guid>> GetFollowerIdsAsync(Guid userId, int page, int pageSize)
        {
            return await _followRepository.GetFollowerIdsAsync(userId, page, pageSize);
        }

        public async Task<IEnumerable<Guid>> GetFollowingIdsAsync(Guid userId, int page, int pageSize)
        {
            return await _followRepository.GetFollowingIdsAsync(userId, page, pageSize);
        }

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
        {
            return await _followRepository.IsFollowingAsync(followerId, followeeId);
        }
    }
}
