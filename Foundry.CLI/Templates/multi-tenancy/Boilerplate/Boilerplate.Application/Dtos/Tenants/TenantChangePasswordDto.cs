namespace Boilerplate.Application.Dtos.Tenants
{
    public record TenantChangePasswordDto(
        string Email,
        string Password    
    );
}
