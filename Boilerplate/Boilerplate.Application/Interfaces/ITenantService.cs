namespace Boilerplate.Application.Interfaces
{
    public interface ITenantService
    {
        Task<bool> InviteUserToTenantAsync(int tenantId, string userEmail);
    }
}
