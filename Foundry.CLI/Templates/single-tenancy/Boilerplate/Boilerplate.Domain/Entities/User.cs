namespace Boilerplate.Domain.Entities
{
    public class User : EntityBase
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<SystemNotificationUser> SystemNotificationUser { get; set; } = new List<SystemNotificationUser>();
    }
}
