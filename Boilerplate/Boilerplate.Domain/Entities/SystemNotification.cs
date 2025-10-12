namespace Boilerplate.Domain.Entities
{
    public class SystemNotification : EntityBase
    {
        public string Content { get; set; }
        public bool IsRead { get; set; }
    }
}
