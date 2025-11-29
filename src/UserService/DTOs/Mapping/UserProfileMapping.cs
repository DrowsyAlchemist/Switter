using AutoMapper;
using UserService.Models;

namespace UserService.DTOs.Mapping
{
    public class UserProfileMapping : Profile
    {
        public UserProfileMapping()
        {
            CreateMap<UserProfile, UserProfileDto>()
                .ForMember(dest => dest.IsFollowing, opt => opt.Ignore())
                .ForMember(dest => dest.IsFollowed, opt => opt.Ignore())
                .ForMember(dest => dest.IsBlocking, opt => opt.Ignore())
                .ForMember(dest => dest.IsBlocked, opt => opt.Ignore());
        }
    }
}
