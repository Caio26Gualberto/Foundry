using Boilerplate.Application.Common.SystemNotifications;
using Boilerplate.Application.Dtos.SystemNotification;
using Boilerplate.Application.Interfaces;
using Boilerplate.Application.Interfaces.ICurrentUserContext;
using Boilerplate.Application.Services.SignalR;
using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Interfaces.Repositories;
using Boilerplate.Domain.Interfaces.Repositories.IUnitOfWork;
using Microsoft.AspNetCore.SignalR;

namespace Boilerplate.Application.Services.Notifications
{
    public class SystemNotificationService : ISystemNotificationService
    {
        private readonly IRepository<SystemNotification> _repository;
        private readonly IRepository<SystemNotificationUser> _repositoryNotificationsUsers;
        private readonly IRepository<User> _userRepository;
        private readonly IHubContext<SystemNotificationHub> _hubContext;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly IUnitOfWork _unitOfWork;
        public SystemNotificationService(IRepository<SystemNotification> repository, IRepository<SystemNotificationUser> repositoryNotificationsUsers,
            IRepository<User> userRepository, IHubContext<SystemNotificationHub> hubContext,
            ICurrentUserContext currentUserContext, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _repositoryNotificationsUsers = repositoryNotificationsUsers;
            _userRepository = userRepository;
            _hubContext = hubContext;
            _currentUserContext = currentUserContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<SystemNotificationDto> CreateSystemNotification(CreateSystemNotificationDto input)
        {
            List<User> users;

            if (input.UserIds.Count == 0)
            {
                users = _userRepository.GetAll().ToList();
            }
            else
            {
                users = _userRepository
                    .GetAll()
                    .Where(x => input.UserIds.Contains(x.Id))
                    .ToList();
            }

            if (users.Count == 0)
                throw new Exception("No valid users found for the notification.");

            var notification = new SystemNotification
            {
                Title = input.Title,
                Content = input.Content,
            };
            await _unitOfWork.BeginTransactionAsync();
            await _repository.AddAsync(notification);
            await _unitOfWork.CommitAsync();

            var userNotifications = users.Select(user => new SystemNotificationUser
            {
                UserId = user.Id,
                NotificationId = notification.Id,
                IsRead = false
            }).ToList();

            await _repositoryNotificationsUsers.AddRangeAsync(userNotifications);
            await _unitOfWork.CommitAsync();

            await _hubContext.Clients
                .Users(users.Select(x => x.Id.ToString()).ToList())
                .SendAsync(SystemNotificationEvents.UpdateNotifications);

            return new SystemNotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Content = notification.Content,
                IsRead = false,
                CreatedAt = notification.CreatedAt,
            };
        }

        public async Task<bool> DeleteAllMessages(ClearAllMessagesDto input)
        {
            var notifications = _repositoryNotificationsUsers.GetAll()
                .Where(nu => input.NotificationIds.Contains(nu.NotificationId) && nu.UserId == _currentUserContext.UserId).ToList();

            if (!notifications.Any())
                throw new Exception("No notifications found to delete.");

            foreach (var notification in notifications)
                await _repositoryNotificationsUsers.SoftDelete(notification);

            await _hubContext.Clients
                .User(_currentUserContext.UserId.ToString())
                .SendAsync(SystemNotificationEvents.UpdateNotifications);

            return true;
        }

        public async Task<List<SystemNotificationDto>> GetAllNotifications()
        {
            var notifications = _repositoryNotificationsUsers.GetAll(n => n.Notification).Where(nu => nu.UserId == _currentUserContext.UserId);
            return notifications.Select(n => new SystemNotificationDto
            {
                Id = n.Notification.Id,
                Title = n.Notification.Title,
                Content = n.Notification.Content,
                IsRead = n.IsRead,
                CreatedAt = n.Notification.CreatedAt,
            }).OrderByDescending(n => n.CreatedAt).ToList();
        }

        public async Task<bool> MarkNotificationAsRead(int id, MarkAsReadDto input)
        {
            var notificationUser = _repositoryNotificationsUsers
                .GetAll()
                .FirstOrDefault(nu => nu.NotificationId == id && nu.UserId == _currentUserContext.UserId);

            if (notificationUser == null)
                throw new Exception("Notification not found for the user.");

            notificationUser.IsRead = input.IsRead;
            notificationUser.ReadAt = input.IsRead ? DateTime.UtcNow : null;

            await _repositoryNotificationsUsers.UpdateAsync(notificationUser);
            await _hubContext.Clients
                .User(_currentUserContext.UserId.ToString())
                .SendAsync(SystemNotificationEvents.UpdateNotifications);

            return true;
        }
    }
}
