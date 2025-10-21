using Boilerplate.Application.Dtos.Auth;
using Boilerplate.Application.Dtos.Tenants;
using Boilerplate.Application.DTOs.Auth;
using Boilerplate.Application.Interfaces;
using Boilerplate.Application.Services.SignalR;
using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Interfaces.Authenticate;
using Boilerplate.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.SignalR;
using NewLevel.Shared.DTOs.Auth;

namespace Boilerplate.Application.Services.Auth
{
    public class AuthAppService
    {
        private readonly IAuthenticateService _authService;
        private readonly IEmailService _emailService;
        private readonly IRepository<User> _userRepository;
        private readonly IHubContext<SystemNotificationHub> _hubContext;

        public AuthAppService(IAuthenticateService authenticateService, IEmailService emailService, IRepository<User> userRepository,
            IHubContext<SystemNotificationHub> hubContext)
        {
            _authService = authenticateService;
            _emailService = emailService;
            _userRepository = userRepository;
            _hubContext = hubContext;
        }

        public async Task<bool> ConfirmEmail(string userId, string token)
        {
            return await _authService.ConfirmEmail(userId, token);
        }

        public async Task<LoginResponseDto> Authenticate(string email, string password)
        {
            var user = _userRepository.GetAll(x => x.Tenant).Where(x => x.Email == email).FirstOrDefault();
            if (user == null)
                throw new Exception("Usuário não encontrado");

            var tenantId = user.TenantId;

            var (isAuthenticated, isNeededChangePassword) = await _authService.Authenticate(email, password);

            if (!isAuthenticated)
            {
                return new LoginResponseDto
                {
                    Tokens = null,
                    IsNeededChangePassword = isNeededChangePassword
                };
            }

            // Generate JWT tokens
            var accessToken = await _authService.GenerateJwtToken(email, user);
            var refreshToken = _authService.GenerateRefreshToken();

            // Save refresh token
            await _authService.SaveRefreshToken(email, refreshToken, tenantId);

            return new LoginResponseDto
            {
                Tokens = new TokensDto
                {
                    Token = accessToken,
                    RefreshToken = refreshToken
                },
                IsNeededChangePassword = isNeededChangePassword
            };
        }

        public async Task<bool> ChangePassword(TenantChangePasswordDto input)
            => await _authService.ChangePassword(input.Email, input.Password);

        public async Task<RegisterResponseDto> Register(RegisterInputDto input)
        {
            var (userId, email) = await _authService.Register(input.Email, input.Password, input.Nickname, (int)input.TenantId!, input.Token);

            if (string.IsNullOrEmpty(email))
            {
                return new RegisterResponseDto
                {
                    Result = false,
                    Message = "Erro ao registrar usuário"
                };
            }

            return new RegisterResponseDto
            {
                Result = true,
                Message = "Usuário registrado com sucesso"
            };
        }

        public async Task<bool> Logout()
        {
            await _hubContext.Clients.User("1")
                .SendAsync("NotificationsUpdated");
            await _authService.Logout();
            return true;
        }

        public async Task<ForgotPasswordResponseDto> ForgotPassword(ForgotPasswordRequestDto request)
        {
            var token = await _authService.GeneratePasswordResetTokenAsync(request.Email);

            if (string.IsNullOrEmpty(token))
            {
                return new ForgotPasswordResponseDto
                {
                    IsSuccess = false,
                    Message = "Email não encontrado no sistema"
                };
            }

            // Enviar email com o token de recuperação
            var emailSent = await _emailService.SendPasswordResetEmailAsync(request.Email, token);

            if (!emailSent)
            {
                return new ForgotPasswordResponseDto
                {
                    IsSuccess = false,
                    Message = "Erro ao enviar email de recuperação. Tente novamente mais tarde."
                };
            }

            return new ForgotPasswordResponseDto
            {
                IsSuccess = true,
                Message = "Email de recuperação enviado com sucesso. Verifique sua caixa de entrada."
            };
        }

        public async Task<ResetPasswordResponseDto> ResetPassword(ResetPasswordRequestDto request)
        {
            var result = await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

            return new ResetPasswordResponseDto
            {
                IsSuccess = result,
                Message = result ? "Senha alterada com sucesso" : "Token inválido ou expirado"
            };
        }

        public async Task<TokensDto> RefreshTokens(string refreshToken)
        {
            if (!await _authService.ValidateRefreshToken(refreshToken))
                return EmptyTokens();

            var email = await _authService.GetEmailFromRefreshToken(refreshToken);
            if (string.IsNullOrEmpty(email))
                return EmptyTokens();

            var user = _userRepository.GetAll(x => x.Tenant).FirstOrDefault(x => x.Email == email);
            if (user == null)
                return EmptyTokens();

            if (!await _authService.IsExpiredRefreshToken(refreshToken))
            {
                var newAccessToken = await _authService.GenerateJwtToken(email, user);
                return new TokensDto
                {
                    Token = newAccessToken
                };
            }

            await _authService.RemoveRefreshToken(refreshToken);

            var accessToken = await _authService.GenerateJwtToken(email, user);
            var newRefreshToken = _authService.GenerateRefreshToken();

            await _authService.SaveRefreshToken(email, newRefreshToken, (int)user.TenantId!);

            return new TokensDto
            {
                Token = accessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<bool> ValidateInviteToken(ValidateInvitationTokenInputDto input)
            => await _authService.ValidateInviteToken(input.Email, input.Token);

        public async Task<bool> AcceptTenantInvite(AcceptTenantInvitationInputDto input)
        {
            var result = await _authService.Register(input.Email, input.Password, input.Name, input.TenantId, input.Token);
            return !string.IsNullOrEmpty(result.Item2);
        }

        private static TokensDto EmptyTokens() => new TokensDto
        {
            Token = string.Empty,
            RefreshToken = string.Empty
        };
    }
}
