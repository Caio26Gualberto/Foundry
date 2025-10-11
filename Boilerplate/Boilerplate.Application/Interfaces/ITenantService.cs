using Boilerplate.Application.Dtos.Tenants;

namespace Boilerplate.Application.Interfaces
{
    public interface ITenantService
    {
        Task<List<TenantDto>> GetAllTenants();
        Task<bool> InviteUserToTenantAsync(int tenantId, string userEmail);
    }
}
