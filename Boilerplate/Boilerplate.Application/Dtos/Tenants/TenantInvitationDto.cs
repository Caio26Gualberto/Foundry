namespace Boilerplate.Application.Dtos.Tenants
{
    public class TenantInvitationDto
    {
        public int TenantId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
