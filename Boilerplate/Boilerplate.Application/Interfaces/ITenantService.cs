using Boilerplate.Application.Dtos.Tenants;
using Boilerplate.Application.DTOs.Auth;
using Boilerplate.Domain.Models;

namespace Boilerplate.Application.Interfaces
{
    public interface ITenantService
    {
        Task<List<TenantDto>> GetAllTenants();
        Task<int> Create(string name, Address address);
        Task<bool> Update(int tenantid, string name, Address address);
        Task<bool> Delete(int tenantId);
        Task<bool> InviteUserToTenantAsync(int tenantId, string userEmail);
        Task<TokensDto> ImpersonateTenantByUser(int userId, int tenantId);
    }
}
