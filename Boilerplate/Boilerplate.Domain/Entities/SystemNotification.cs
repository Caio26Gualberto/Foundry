namespace Boilerplate.Domain.Entities
{
    public class SystemNotification : EntityBase
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public List<User> Users { get; set; } = new List<User>();
    }
}
