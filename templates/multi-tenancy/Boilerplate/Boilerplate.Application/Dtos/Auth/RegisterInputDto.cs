namespace Boilerplate.Application.DTOs.Auth
{
    public class RegisterInputDto
    {
        public string Email { get; set; } = string.Empty;
        public int? TenantId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
