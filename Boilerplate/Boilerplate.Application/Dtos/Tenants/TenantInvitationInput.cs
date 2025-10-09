namespace Boilerplate.Application.Dtos.Tenants
{
    public class TenantInvitationInput
    {
        public int TenantId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
