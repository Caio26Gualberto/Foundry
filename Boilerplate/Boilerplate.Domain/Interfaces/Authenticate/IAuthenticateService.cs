using Boilerplate.Domain.Entities;
using System.Security.Claims;

namespace Boilerplate.Domain.Interfaces.Authenticate
{
    public interface IAuthenticateService
    {
        Task<(bool, bool)> Authenticate(string email, string password);
        Task<(int, string)> Register(string email, string password, string name, int tenantId, string token);
        Task<(int, string)> RegisterTenantAdmin(string email, string password, string name, int tenantId);
        Task Logout();
        Task<string?> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> ChangePassword(string email, string password);

        Task<bool> ConfirmEmail(string userId, string token);

        // JWT Methods
        Task<string> GenerateJwtToken(string email, User domainUser);
        Task<string> GenerateJwtImpersonatorToken(User domainUser, User targetUser);
        ClaimsPrincipal GetClaimsByToken(string impersonatedToken);
        string GenerateRefreshToken();
        Task<bool> ValidateRefreshToken(string refreshToken);
        Task<string?> GetEmailFromRefreshToken(string refreshToken);
        Task<bool> IsExpiredRefreshToken(string refreshToken);
        Task SaveRefreshToken(string email, string refreshToken, int? tenantId);
        Task<bool> RemoveRefreshToken(string refreshToken);
        Task<bool> ValidateInviteToken(string email, string token);
    }
}
