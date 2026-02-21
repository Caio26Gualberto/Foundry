namespace Boilerplate.Application.Dtos.SystemNotification
{
    public record CreateSystemNotificationDto(
        string Title,
        string Content,
        List<int> UserIds
    );
}
