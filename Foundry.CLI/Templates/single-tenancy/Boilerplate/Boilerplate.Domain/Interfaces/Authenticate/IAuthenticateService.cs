using Boilerplate.Domain.Entities;

namespace Boilerplate.Domain.Interfaces.Authenticate
{
    public interface IAuthenticateService
    {
        Task<(bool, bool)> Authenticate(string email, string password);
        Task<(int, string)> Register(string email, string password, string name);
        Task Logout();
        Task<string?> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> ChangePassword(string email, string password);

        Task<bool> ConfirmEmail(string userId, string token);
        Task<string> GenerateEmailVerificationCode(string email);
        Task<bool> VerifyEmailCode(string email, string code);
        Task<bool> IsEmailConfirmed(string email);

        // JWT Methods
        Task<string> GenerateJwtToken(string email, User domainUser);
        string GenerateRefreshToken();
        Task<bool> ValidateRefreshToken(string refreshToken);
        Task<string?> GetEmailFromRefreshToken(string refreshToken);
        Task<bool> IsExpiredRefreshToken(string refreshToken);
        Task SaveRefreshToken(string email, string refreshToken);
        Task<bool> RemoveRefreshToken(string refreshToken);
    }
}
