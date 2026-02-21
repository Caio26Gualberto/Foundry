namespace Boilerplate.Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        public TokensDto? Tokens { get; set; }
        public bool IsNeededChangePassword { get; set; }
    }
}
