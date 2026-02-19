namespace Boilerplate.Domain.Entities
{
    public class SystemNotification : EntityBase
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? TenantId { get; set; }
        public List<SystemNotificationUser> SystemNotificationUser { get; set; } = new List<SystemNotificationUser>();
    }
}
