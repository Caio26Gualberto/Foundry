using Boilerplate.Application.Common.SystemNotifications;
using Boilerplate.Application.Dtos.SystemNotification;
using Boilerplate.Application.Interfaces;
using Boilerplate.Application.Services.SignalR;
using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace Boilerplate.Application.Services.Notifications
{
    public class SystemNotificationService : ISystemNotificationService
    {
        private readonly IRepository<SystemNotification> _repository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IHubContext<SystemNotificationHub> _hubContext;
        public SystemNotificationService(IRepository<SystemNotification> repository, IRepository<User> userRepository, IRepository<Tenant> tenantRepository,
            IHubContext<SystemNotificationHub> hubContext)
        {
            _repository = repository;
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
            _hubContext = hubContext;
        }

        public async Task<SystemNotificationDto> CreateSystemNotification(CreateSystemNotificationDto input)
        {
            var users = new List<User>();
            if (input.UserIds.Count == 0)
                users = _tenantRepository.GetAll().Where(t => t.Users.Any()).SelectMany(t => t.Users).ToList();
            else
                users = _userRepository.GetAll().Where(x => input.UserIds.Contains(x.Id)).ToList();

            if (users == null || users.Count() == 0)
                throw new Exception("No valid users found for the notification.");

            var notification = new SystemNotification
            {
                Title = input.Title,
                Content = input.Content,
                Users = users,
            };

            await _repository.AddAsync(notification);

            await _hubContext.Clients.Users(users.Select(x => x.Id.ToString()).ToList())
                .SendAsync(SystemNotificationEvents.UpdateNotifications);

            return new SystemNotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Content = notification.Content,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
            };
        }

        public async Task<List<SystemNotificationDto>> GetAllNotifications()
        {
            var notifications = _repository.GetAll();
            return notifications.Select(n => new SystemNotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
            }).ToList();
        }
    }
}
