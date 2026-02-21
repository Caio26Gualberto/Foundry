using Boilerplate.Domain.Enums;

namespace Boilerplate.Domain.Entities
{
    public class TenantInvitation : EntityBase
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public int TenantId { get; set; }
        public EInviteStatus Status { get; set; } = EInviteStatus.Pending;
        public DateTime ExpiresAt { get; set; }
    }
}
