using Boilerplate.Application.DTOs.Auth;
using Boilerplate.Domain.Models;

namespace Boilerplate.Application.Dtos.Tenants
{
    public record TenantCreateOrUpdateDto(
        string Name,
        Address Address,
        RegisterInputDto RegisterInput
    );
}
