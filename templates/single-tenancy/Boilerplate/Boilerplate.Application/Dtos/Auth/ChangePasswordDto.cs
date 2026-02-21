namespace Boilerplate.Application.Dtos.Auth
{
    public record ChangePasswordDto(
        string Email,
        string Password    
    );
}
