using Boilerplate.Application.Common.Results;
using Boilerplate.Application.Dtos.Auth;
using Boilerplate.Application.Dtos.Tenants;
using Boilerplate.Application.DTOs.Auth;
using Boilerplate.Application.Interfaces;
using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Interfaces.Authenticate;
using Boilerplate.Domain.Interfaces.Repositories;
using NewLevel.Shared.DTOs.Auth;

namespace Boilerplate.Application.Services.Auth
{
    public class AuthAppService
    {
        private readonly IAuthenticateService _authService;
        private readonly IEmailService _emailService;
        private readonly IRepository<User> _userRepository;

        public AuthAppService(IAuthenticateService authenticateService, IEmailService emailService, IRepository<User> userRepository)
        {
            _authService = authenticateService;
            _emailService = emailService;
            _userRepository = userRepository;
        }

        public async Task<bool> ConfirmEmail(string userId, string token)
        {
            return await _authService.ConfirmEmail(userId, token);
        }

        public async Task<Result<LoginResponseDto>> Authenticate(string email, string password)
        {
            var user = _userRepository.GetAll(x => x.Tenant!).Where(x => x.Email == email).FirstOrDefault();

            if (user == null)
                return Result<LoginResponseDto>.Fail(
                    new Error("USER_NOT_FOUND", "User not found", ErrorType.NotFound)
                );

            var tenantId = user.TenantId;

            var (isAuthenticated, isNeededChangePassword) = await _authService.Authenticate(email, password);

            var response = new LoginResponseDto();

            if (!isAuthenticated)
            {
                response = new LoginResponseDto
                {
                    Tokens = null,
                    IsNeededChangePassword = isNeededChangePassword
                };

                return Result<LoginResponseDto>.Fail(
                    new Error("INVALID_CREDENTIALS", "Invalid email or password", ErrorType.Unauthorized)
                );
            }

            var accessToken = await _authService.GenerateJwtToken(email, user);
            var refreshToken = _authService.GenerateRefreshToken();

            await _authService.SaveRefreshToken(email, refreshToken, tenantId);

            response = new LoginResponseDto
            {
                Tokens = new TokensDto
                {
                    Token = accessToken,
                    RefreshToken = refreshToken
                },
                IsNeededChangePassword = isNeededChangePassword
            };

            return Result<LoginResponseDto>.Ok(response);
        }

        public async Task<bool> ChangePassword(TenantChangePasswordDto input)
            => await _authService.ChangePassword(input.Email, input.Password);

        public async Task<Result<RegisterResponseDto>> Register(RegisterInputDto input)
        {
            var (userId, email) = await _authService.Register(input.Email, input.Password, input.Nickname, (int)input.TenantId!, input.Token);

            if (string.IsNullOrEmpty(email))
                return Result<RegisterResponseDto>.Fail(
                    new Error("REGISTRATION_FAILED", "Failed to register user", ErrorType.Validation)
                );

            var response = new RegisterResponseDto
            {
                Result = true,
                Message = "User registered successfully"
            };

            return Result<RegisterResponseDto>.Ok(response);
        }

        public async Task<bool> Logout()
        {
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

        public async Task<Result<ResetPasswordResponseDto>> ResetPassword(ResetPasswordRequestDto request)
        {
            var result = await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

            if (!result)
                return Result<ResetPasswordResponseDto>.Fail(
                    new Error("INVALID_TOKEN", "Token inválido ou expirado", ErrorType.Unauthorized)
                );

            var response = new ResetPasswordResponseDto
            {
                IsSuccess = result,
                Message = result ? "Senha alterada com sucesso" : "Token inválido ou expirado"
            };

            return Result<ResetPasswordResponseDto>.Ok(response);
        }

        public async Task<Result<TokensDto>> RefreshTokens(string refreshToken)
        {
            if (!await _authService.ValidateRefreshToken(refreshToken))
                return Result<TokensDto>.Fail(
                    new Error("INVALID_TOKEN", "Invalid or expired token", ErrorType.Unauthorized)    
                );

            var email = await _authService.GetEmailFromRefreshToken(refreshToken);

            if (string.IsNullOrEmpty(email))
                return Result<TokensDto>.Fail(
                    new Error("EMAIL_NOT_FOUND", "Email not found in token", ErrorType.Unauthorized)
                );

            var user = _userRepository.GetAll(x => x.Tenant!).FirstOrDefault(x => x.Email == email);

            if (user == null)
                return Result<TokensDto>.Fail(
                    new Error("USER_NOT_FOUND", "User not found", ErrorType.NotFound)
                );

            var response = new TokensDto();

            if (!await _authService.IsExpiredRefreshToken(refreshToken))
            {
                var newAccessToken = await _authService.GenerateJwtToken(email, user);

                response = new TokensDto
                {
                    Token = newAccessToken
                };

                return Result<TokensDto>.Ok(response);
            }

            await _authService.RemoveRefreshToken(refreshToken);

            var accessToken = await _authService.GenerateJwtToken(email, user);
            var newRefreshToken = _authService.GenerateRefreshToken();

            await _authService.SaveRefreshToken(email, newRefreshToken, (int)user.TenantId!);

            response = new TokensDto
            {
                Token = accessToken,
                RefreshToken = newRefreshToken
            };

            return Result<TokensDto>.Ok(response);
        }

        public async Task<bool> ValidateInviteToken(ValidateInvitationTokenInputDto input)
            => await _authService.ValidateInviteToken(input.Email, input.Token);

        public async Task<Result<bool>> AcceptTenantInvite(AcceptTenantInvitationInputDto input)
        {
            var result = await _authService.Register(input.Email, input.Password, input.Name, input.TenantId, input.Token);

            if (string.IsNullOrEmpty(result.Item2))
                return Result<bool>.Fail(
                    new Error("INVALID_TOKEN", "Invalid or expired token", ErrorType.Unexpected)
                );

            return Result<bool>.Ok(true);
        }
    }
}
