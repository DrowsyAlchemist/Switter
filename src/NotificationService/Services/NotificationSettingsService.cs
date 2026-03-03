using AutoMapper;
using NotificationService.DTOs;
using NotificationService.Interfaces;
using NotificationService.Interfaces.Data;
using NotificationService.Models;

namespace NotificationService.Services
{
    public class NotificationSettingsService : INotificationSettingsService
    {
        private readonly INotificationSettingsRepository _repository;
        private readonly IMapper _mapper;

        public NotificationSettingsService(INotificationSettingsRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<UserNotificationSettingsDto> GetSettingsAsync(Guid userId)
        {
            var userSettings = await _repository.GetAsync(userId);
            return _mapper.Map<UserNotificationSettingsDto>(userSettings);
        }

        public async Task<UserNotificationSettingsDto> UpdateSettingsAsync(UserNotificationSettingsDto settingsDto)
        {
            var notificationSettings = _mapper.Map<UserNotificationSettings>(settingsDto);
            await _repository.UpdateAsync(notificationSettings);
            return settingsDto;
        }
    }
}
