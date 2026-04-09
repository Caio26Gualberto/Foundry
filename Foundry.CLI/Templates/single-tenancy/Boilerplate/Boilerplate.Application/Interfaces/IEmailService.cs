using Boilerplate.Application.DTOs.Email;

namespace Boilerplate.Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailDto emailDto);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken);
        Task<bool> SendEmailVerification(string token, int userId, string email);
        Task<bool> SendVerificationCodeEmail(string email, string code);
    }
}
