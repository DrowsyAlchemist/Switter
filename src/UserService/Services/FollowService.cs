using AutoMapper;
using UserService.DTOs;
using UserService.Exceptions.Follows;
using UserService.Interfaces;
using UserService.Interfaces.Data;

namespace UserService.Services
{
    public class FollowService : IFollowService, IFollowChecker
    {
        private readonly IFollowRepository _followRepository;
        private readonly IUserRelationshipService _relationshipService;
        private readonly IMapper _mapper;

        public FollowService(IFollowRepository followRepository, IUserRelationshipService relationshipService, IMapper mapper)
        {
            _followRepository = followRepository;
            _relationshipService = relationshipService;
            _mapper = mapper;
        }

        public async Task FollowUserAsync(Guid followerId, Guid followeeId)
        {
            if (followerId == followeeId)
                throw new SelfFollowException();

            bool isFollowing = await _followRepository.IsFollowingAsync(followerId, followeeId);
            if (isFollowing)
                throw new DoubleFollowException();

            if (await _relationshipService.IsBlockedAsync(followerId, followeeId))
                throw new FollowToBlockedUserException();
            if (await _relationshipService.IsBlockedAsync(followeeId, followerId))
                throw new FollowToBlockerException();

            await _followRepository.AddAsync(followerId, followeeId);
        }

        public async Task UnfollowUserAsync(Guid followerId, Guid followeeId)
        {
            bool isFollowing = await _followRepository.IsFollowingAsync(followerId, followeeId);
            if (isFollowing == false)
                throw new FollowNotFoundException(followerId, followeeId);

            await _followRepository.DeleteAsync(followerId, followeeId);
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
