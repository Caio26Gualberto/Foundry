using Boilerplate.Domain.Entities;

namespace Boilerplate.Domain.Interfaces.Authenticate
{
    public interface IAuthenticateService
    {
        Task<bool> Authenticate(string email, string password);
        Task<(int, string)> Register(string email, string password, string nickname, int tenantId, string token);
        Task Logout();
        Task<string?> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> ConfirmEmail(string userId, string token);

        // JWT Methods
        Task<string> GenerateJwtToken(string email, User domainUser);
        string GenerateRefreshToken();
        Task<bool> ValidateRefreshToken(string refreshToken);
        Task<string?> GetEmailFromRefreshToken(string refreshToken);
        Task SaveRefreshToken(string email, string refreshToken, int? tenantId);
        Task<bool> RemoveRefreshToken(string refreshToken);
    }
}
