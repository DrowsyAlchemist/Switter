using AutoMapper;
using UserService.DTOs;
using UserService.Events;
using UserService.Exceptions.Follows;
using UserService.Interfaces;
using UserService.Interfaces.Data;
using UserService.Interfaces.Infrastructure;

namespace UserService.Services
{
    public class FollowService : IFollowService
    {
        private readonly IFollowRepository _followRepository;
        private readonly IFollowersCounter _followersCounter;
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly IMapper _mapper;

        public FollowService(IFollowRepository followRepository, IFollowersCounter followersCounter, IKafkaProducerService kafkaProducer, IMapper mapper)
        {
            _followRepository = followRepository;
            _followersCounter = followersCounter;
            _kafkaProducer = kafkaProducer;
            _mapper = mapper;
        }

        public async Task FollowUserAsync(Guid followerId, Guid followeeId)
        {
            if (followerId == followeeId)
                throw new SelfFollowException();

            bool isFollowing = await _followRepository.IsFollowingAsync(followerId, followeeId);
            if (isFollowing)
                throw new DoubleFollowException();

            await _followRepository.AddAsync(followerId, followeeId);
            await _followersCounter.IncrementCounter(followerId, followeeId);
            await _kafkaProducer.ProduceAsync("user-events", new UserFollowedEvent
            {
                FollowerId = followerId,
                FolloweeId = followeeId,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task UnfollowUserAsync(Guid followerId, Guid followeeId)
        {
            bool isFollowing = await _followRepository.IsFollowingAsync(followerId, followeeId);
            if (isFollowing == false)
                throw new FollowNotFoundException(followerId, followeeId);

            await _followRepository.DeleteAsync(followerId, followeeId);
            await _followersCounter.DecrementCounter(followerId, followeeId);
            await _kafkaProducer.ProduceAsync("user-events", new UserUnfollowedEvent
            {
                FollowerId = followerId,
                FolloweeId = followeeId,
                Timestamp = DateTime.UtcNow
            });
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
    }
}
