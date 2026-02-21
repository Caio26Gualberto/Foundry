namespace Boilerplate.Application.Dtos.SystemNotification
{
    public record ClearAllMessagesDto(
        List<int> NotificationIds    
    );
}
