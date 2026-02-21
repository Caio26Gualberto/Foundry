namespace Boilerplate.Application.Dtos.Auth
{
    public record ValidateInvitationTokenInputDto(
        string Email,
        string Token
    );
}
