using Boilerplate.Application.Common.Results;
using Boilerplate.Application.Dtos.SystemNotification;

namespace Boilerplate.Application.Interfaces
{
    public interface ISystemNotificationService
    {
        Task<Result<List<SystemNotificationDto>>> GetAllNotifications();
        Task<Result<SystemNotificationDto>> CreateSystemNotification(CreateSystemNotificationDto input);
        Task<Result<bool>> MarkNotificationAsRead(int id, MarkAsReadDto input);
        Task<Result<bool>> DeleteAllMessages(ClearAllMessagesDto input);
    }
}
