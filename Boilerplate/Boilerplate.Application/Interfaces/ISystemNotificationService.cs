using Boilerplate.Application.Dtos.SystemNotification;

namespace Boilerplate.Application.Interfaces
{
    public interface ISystemNotificationService
    {
        Task<List<SystemNotificationDto>> GetAllNotifications();
        Task<SystemNotificationDto> CreateSystemNotification(CreateSystemNotificationDto input);
    }
}
