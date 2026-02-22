namespace Boilerplate.Domain.Entities
{
    public class SystemNotificationUser : EntityBase
    {
        public int UserId { get; set; }
        public User User { get; set; } = new User();

        public int NotificationId { get; set; }
        public SystemNotification Notification { get; set; } = new SystemNotification();

        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
