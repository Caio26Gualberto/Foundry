namespace Boilerplate.Application.Dtos.Users
{
    public record UpdateUserDto(
        string Name,
        string Email,
        List<string> Roles
    );
}
