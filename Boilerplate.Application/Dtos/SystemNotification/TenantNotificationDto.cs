namespace Boilerplate.Application.Dtos.SystemNotification
{
    public class TenantNotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int UsersCount { get; set; }
        public int ReadCount { get; set; }
    }
}
