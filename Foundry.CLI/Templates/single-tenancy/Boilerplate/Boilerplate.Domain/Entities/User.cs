namespace Boilerplate.Domain.Entities
{
    public class User : EntityBase
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public List<SystemNotificationUser> SystemNotificationUser { get; set; } = new List<SystemNotificationUser>();
    }
}
