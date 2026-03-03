using AutoMapper;
using NotificationService.Models;

namespace NotificationService.DTOs.Mapping
{
    public class UserNotificationMapping:Profile
    {
        public UserNotificationMapping()
        {
            CreateMap<Notification, NotificationDto>();

            CreateMap<UserNotificationSettingsDto, UserNotificationSettings>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UserNotificationSettings, UserNotificationSettingsDto>();
        }
    }
}
