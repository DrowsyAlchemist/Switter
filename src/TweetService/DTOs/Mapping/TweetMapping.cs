using AutoMapper;
using TweetService.Models;

namespace TweetService.DTOs.Mapping
{
    public class TweetMapping : Profile
    {
        public TweetMapping()
        {
            CreateMap<Tweet, TweetDto>()
                .ForMember(dest => dest.IsLiked, opt => opt.Ignore())
                .ForMember(dest => dest.IsRetweeted, opt => opt.Ignore());
        }
    }
}
