using UserService.Exceptions.Follows;
using UserService.Interfaces;
using UserService.Interfaces.Commands;
using UserService.Interfaces.Data;

namespace UserService.Services.Commands
{
    public class FollowCommands : IFollowCommands
    {
        private readonly IFollowRepository _followRepository;
        private readonly IUserRelationshipService _relationshipService;

        public FollowCommands(IFollowRepository followRepository, IUserRelationshipService relationshipService)
        {
            _followRepository = followRepository;
            _relationshipService = relationshipService;
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

    }
}
