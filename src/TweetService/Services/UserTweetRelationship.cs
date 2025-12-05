using TweetService.Data;
using TweetService.DTOs;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Services;

namespace TweetService.Services
{
    public class UserTweetRelationship : IUserTweetRelationship
    {
       // private readonly TweetDbContext _context;
        private readonly ITweetRepository _tweetRepository;
        private readonly ILikesRepository _likesRepository;

        public UserTweetRelationship(ITweetRepository tweetRepository, ILikesRepository likesRepository)
        {
           // _context = context;
            _tweetRepository = tweetRepository;
            _likesRepository = likesRepository;
        }

        public async Task<TweetDto> GetTweetWithRelationshipsAsync(TweetDto tweetDto, Guid userId)
        {
            tweetDto.IsLiked = await _likesRepository.IsExist(userId, tweetDto.Id);
            tweetDto.IsRetweeted = await _tweetRepository.IsRetweetedAsync(userId, tweetDto.Id);
            return tweetDto;
        }

        //public async Task<List<Guid>> GetLiked(Guid userId, List<Guid> tweets)
        //{
        //   // return await _context.
        //}
    }
}
