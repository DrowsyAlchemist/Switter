using AutoMapper;
using NotificationService.DTOs;
using NotificationService.Exceptions;
using NotificationService.Interfaces;
using NotificationService.Interfaces.Data;

namespace NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, int page, int pageSize)
        {
            var notifications = await _repository.GetByUserAsync(userId, page, pageSize);
            return _mapper.Map<List<NotificationDto>>(notifications);

        }

        public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(Guid userId, int page, int pageSize)
        {
            var notifications = await _repository.GetUnreadByUserAsync(userId, page, pageSize);
            return _mapper.Map<List<NotificationDto>>(notifications);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _repository.GetUnreadCountByUserAsync(userId);
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _repository.GetByIdAsync(notificationId);
            if (notification == null)
                throw new NotificationNotFoundException(notificationId);
            if (notification.UserId != userId)
                throw new NotificationOwnerNotMatchException(notificationId, userId);
            if (notification.Status == Models.NotificationStatus.Read)
                throw new NotificationAlreadyReadException(notificationId);

            notification.Status = Models.NotificationStatus.Read;
            await _repository.UpdateAsync(notification);
        }

        public async Task<int> MarkAllAsReadAsync(Guid userId)
        {
            return await _repository.MarkAllAsReadAsync(userId);
        }

        public async Task DeleteNotificationAsync(Guid notificationId, Guid userId)
        {
            var notification = await _repository.GetByIdAsync(notificationId);
            if (notification == null)
                throw new NotificationNotFoundException(notificationId);
            if (notification.UserId != userId)
                throw new NotificationOwnerNotMatchException(notificationId, userId);

            await _repository.RemoveAsync(notificationId);
        }
    }
}
