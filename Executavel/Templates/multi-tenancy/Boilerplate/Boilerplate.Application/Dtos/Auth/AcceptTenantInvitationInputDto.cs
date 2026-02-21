namespace Boilerplate.Application.Dtos.Auth
{
    public record AcceptTenantInvitationInputDto(
        string Token,
        string Email,
        string Name,
        string Password, 
        int TenantId
    );
}
