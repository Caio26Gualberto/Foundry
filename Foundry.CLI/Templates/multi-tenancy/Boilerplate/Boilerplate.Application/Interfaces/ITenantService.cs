using Boilerplate.Application.Common.Results;
using Boilerplate.Application.Dtos.Tenants;
using Boilerplate.Application.DTOs.Auth;
using Boilerplate.Domain.Models;

namespace Boilerplate.Application.Interfaces
{
    public interface ITenantService
    {
        Task<Result<List<TenantDto>>> GetAllTenants();
        Task<Result<int>> Create(string name, Address address, RegisterInputDto registerDto);
        Task<Result<bool>> Update(int id, string name, Address address);
        Task<Result<bool>> Delete();
        Task<Result<bool>> InviteUserToTenantAsync(int tenantId, string userEmail);
        Task<Result<TokensDto>> ImpersonateTenantByUser(int userId, int tenantId);
    }
}
