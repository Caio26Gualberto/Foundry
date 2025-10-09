namespace Boilerplate.Domain.Entities
{
    public class TenantInvitation : EntityBase
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public int TenantId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
