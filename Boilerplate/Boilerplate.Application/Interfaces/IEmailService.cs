using Boilerplate.Application.DTOs.Email;
using Boilerplate.Domain.Entities;

namespace Boilerplate.Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendTenantInvitationEmailAsync(string email, Tenant tenant, string invitationToken);
        Task<bool> SendEmailAsync(EmailDto emailDto);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken);
        Task<bool> SendEmailVerification(string token, int userId, string email);
    }
}
