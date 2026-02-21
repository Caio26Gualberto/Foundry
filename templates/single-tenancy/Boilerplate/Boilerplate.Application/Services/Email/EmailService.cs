using Boilerplate.Application.DTOs.Email;
using Boilerplate.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Boilerplate.Application.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
public async Task<bool> SendEmailAsync(EmailDto emailDto)
        {
            try
            {
                using var client = CreateSmtpClient();

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["SMTP:FromEmail"] ?? "", _configuration["SMTP:FromName"] ?? "Boilerplate"),
                    Subject = emailDto.Subject,
                    Body = emailDto.Body,
                    IsBodyHtml = emailDto.IsHtml
                };

                mailMessage.To.Add(emailDto.To);

                await client.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao enviar email", ex);
            }
        }

        public async Task<bool> SendEmailVerification(string token, int userId, string email)
        {
            var basePath = _configuration["Frontend:Url"];

            var confirmationLink = $"{basePath}/confirm-email?userId={userId}&token={Uri.EscapeDataString(token)}";

            var emailBody = BuildEmailTemplate(
                title: "Confirme seu email para ativar sua conta",
                messageBody: @"Olá,<br/><br/>
                    Recebemos uma solicitação de registro com este email. 
                    Para confirmar sua conta e ativar o acesso, clique no botão abaixo:",
                ctaText: "Confirmar Email",
                ctaLink: confirmationLink
            );

            return await SendEmailAsync(new EmailDto
            {
                To = email,
                Subject = "Boilerplate - Confirmação de Email",
                Body = emailBody,
                IsHtml = true
            });
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken)
        {
            var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:3000";
            var resetLink = $"{frontendUrl}/security/resetPassword?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(email)}";

            var emailBody = BuildEmailTemplate(
                "Solicitação de Recuperação de Senha",
                "Você solicitou a recuperação de sua senha. Clique no botão abaixo para redefinir sua senha. Este link expira em 24 horas.",
                "Redefinir Senha",
                resetLink
            );

            var emailDto = new EmailDto
            {
                To = email,
                Subject = "Boilerplate - Recuperação de Senha",
                Body = emailBody,
                IsHtml = true
            };

            return await SendEmailAsync(emailDto);
        }

        private SmtpClient CreateSmtpClient()
        {
            var smtpSettings = _configuration.GetSection("SMTP");

            return new SmtpClient(smtpSettings["Host"], int.Parse(smtpSettings["Port"] ?? "587"))
            {
                Credentials = new NetworkCredential(
                    smtpSettings["Username"],
                    smtpSettings["Password"]
                ),
                EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true")
            };
        }

        public string BuildEmailTemplate(string title, string messageBody, string? ctaText = null, string? ctaLink = null)
        {
            var emailHtml = $@"
                <html>
                <body style='font-family: Arial, sans-serif; margin:0; padding:0; background-color:#f5f5f5;'>
                    <!-- Header -->
                    <div style='background-color:#d32f2f; color:white; padding:20px; text-align:center;'>
                        <h1>Boilerplate</h1>
                    </div>

                    <!-- Body -->
                    <div style='max-width:600px; margin:20px auto; background-color:white; padding:20px; border-radius:5px; box-shadow:0 0 5px rgba(0,0,0,0.1);'>
                        <h2 style='color:#333;'>{title}</h2>
                        <p style='color:#555; line-height:1.5;'>{messageBody}</p>";

            if (!string.IsNullOrEmpty(ctaText) && !string.IsNullOrEmpty(ctaLink))
            {
                emailHtml += $@"
                        <div style='text-align:center; margin:30px 0;'>
                            <a href='{ctaLink}' style='background-color:#d32f2f; color:white; padding:12px 30px; text-decoration:none; border-radius:5px; display:inline-block;'>
                                {ctaText}
                            </a>
                        </div>";
            }

            emailHtml += @"
                        <hr style='border:none; border-top:1px solid #ddd; margin:30px 0;'/>
                        <p style='color:#999; font-size:12px; line-height:1.4;'>
                            Este é um e-mail automático, não responda esta mensagem.<br/>
                            Boilerplate
                        </p>
                    </div>

                    <!-- Footer -->
                    <div style='text-align:center; color:#999; font-size:12px; margin:20px 0;'>
                        &copy; " + DateTime.Now.Year + @" Boilerplate. Todos os direitos reservados.
                    </div>
                </body>
                </html>";

            return emailHtml;
        }
    }
}
