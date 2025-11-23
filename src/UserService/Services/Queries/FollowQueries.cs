using AutoMapper;
using UserService.Interfaces.Data;
using UserService.Interfaces;
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

        public async Task<List<UserProfileDto>> GetFollowersAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var followers = await _followRepository.GetFollowersAsync(userId);
            followers = followers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return _mapper.Map<List<UserProfileDto>>(followers);
        }

        public async Task<List<UserProfileDto>> GetFollowingAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var followings = await _followRepository.GetFollowingsAsync(userId);
            followings = followings.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return _mapper.Map<List<UserProfileDto>>(followings);
        }

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
        {
            return await _followRepository.IsFollowingAsync(followerId, followeeId);
        }
    }
}
