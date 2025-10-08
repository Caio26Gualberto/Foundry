namespace Boilerplate.Domain.Interfaces.Authenticate
{
    public interface IAuthenticateService
    {
        Task<bool> Authenticate(string email, string password, int tenantId);
        Task<(string, int, string)> Register(string email, string password, string nickname);
        Task Logout();
        Task<string?> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> ConfirmEmail(string userId, string token);

        // JWT Methods
        Task<string> GenerateJwtToken(string email, int tenantId);
        Task<string> GenerateRefreshToken();
        Task<bool> ValidateRefreshToken(string refreshToken);
        Task<string?> GetEmailFromRefreshToken(string refreshToken);
        Task SaveRefreshToken(string email, string refreshToken, int tenantId);
        Task<bool> RemoveRefreshToken(string refreshToken);
    }
}
