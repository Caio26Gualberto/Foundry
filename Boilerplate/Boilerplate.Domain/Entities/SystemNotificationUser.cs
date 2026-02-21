namespace Boilerplate.Domain.Entities
{
    public class SystemNotificationUser : EntityBase
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int NotificationId { get; set; }
        public SystemNotification Notification { get; set; }

        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
