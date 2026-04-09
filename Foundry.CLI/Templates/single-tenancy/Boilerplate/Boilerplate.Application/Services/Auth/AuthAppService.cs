using Boilerplate.Application.Common.Results;
using Boilerplate.Application.Dtos.Auth;
using Boilerplate.Application.DTOs.Auth;
using Boilerplate.Application.Interfaces;
using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Interfaces.Authenticate;
using Boilerplate.Domain.Interfaces.Repositories;
using Boilerplate.Domain.Interfaces.Repositories.IUnitOfWork;

namespace Boilerplate.Application.Services.Auth
{
    public class AuthAppService
    {
        private readonly IAuthenticateService _authService;
        private readonly IEmailService _emailService;
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AuthAppService(
            IAuthenticateService authenticateService,
            IEmailService emailService,
            IRepository<User> userRepository,
            IUnitOfWork unitOfWork)
        {
            _authService = authenticateService;
            _emailService = emailService;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> ConfirmEmail(string userId, string token)
        {
            return await _authService.ConfirmEmail(userId, token);
        }

        public async Task<Result<LoginResponseDto>> Authenticate(string email, string password)
        {
            var user = _userRepository.GetAll().Where(x => x.Email == email).FirstOrDefault();
            if (user == null)
                return Result<LoginResponseDto>.Fail(
                    new Error("USER_NOT_FOUND", "User not found", ErrorType.NotFound)
                );

            var (isAuthenticated, isNeededChangePassword) = await _authService.Authenticate(email, password);

            if (!isAuthenticated)
            {
                return Result<LoginResponseDto>.Fail(
                    new Error("INVALID_CREDENTIALS", "Invalid email or password", ErrorType.Unauthorized)
                );
            }

            var isEmailConfirmed = await _authService.IsEmailConfirmed(email);
            if (!isEmailConfirmed)
            {
                return Result<LoginResponseDto>.Fail(
                    new Error("EMAIL_NOT_CONFIRMED", "Email not confirmed. Please verify your email.", ErrorType.Unauthorized)
                );
            }

            var accessToken = await _authService.GenerateJwtToken(email, user);
            var refreshToken = _authService.GenerateRefreshToken();

            await _authService.SaveRefreshToken(email, refreshToken);

            var response = new LoginResponseDto
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

        public async Task<bool> ChangePassword(ChangePasswordDto input)
            => await _authService.ChangePassword(input.Email, input.Password);

        public async Task<Result<RegisterResponseDto>> Register(RegisterInputDto input)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var (_, email) = await _authService.Register(input.Email, input.Password, input.Nickname);

                if (string.IsNullOrEmpty(email))
                {
                    await _unitOfWork.RollbackAsync();
                    return Result<RegisterResponseDto>.Fail(
                        new Error("REGISTRATION_FAILED", "Failed to register user", ErrorType.Validation)
                    );
                }

                var code = await _authService.GenerateEmailVerificationCode(email);
                if (string.IsNullOrEmpty(code))
                {
                    await _unitOfWork.RollbackAsync();
                    return Result<RegisterResponseDto>.Fail(
                        new Error("REGISTRATION_FAILED", "Failed to register user", ErrorType.Validation)
                    );
                }

                try
                {
                    var emailSent = await _emailService.SendVerificationCodeEmail(email, code);
                    if (!emailSent)
                    {
                        await _unitOfWork.RollbackAsync();
                        return Result<RegisterResponseDto>.Fail(
                            new Error("EMAIL_SEND_FAILED", "Failed to send verification email. Please try again later.", ErrorType.Unexpected)
                        );
                    }
                }
                catch
                {
                    await _unitOfWork.RollbackAsync();
                    return Result<RegisterResponseDto>.Fail(
                        new Error("EMAIL_SEND_FAILED", "Failed to send verification email. Please try again later.", ErrorType.Unexpected)
                    );
                }

                await _unitOfWork.CommitAsync();

                var response = new RegisterResponseDto
                {
                    Result = true,
                    Message = "User registered successfully. Please check your email for the verification code."
                };

                return Result<RegisterResponseDto>.Ok(response);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<Result<bool>> VerifyEmail(string email, string code)
        {
            var result = await _authService.VerifyEmailCode(email, code);

            if (!result)
                return Result<bool>.Fail(
                    new Error("INVALID_CODE", "Invalid or expired verification code", ErrorType.Validation)
                );

            return Result<bool>.Ok(true);
        }

        public async Task<Result<bool>> ResendVerificationCode(string email)
        {
            var user = _userRepository.GetAll().FirstOrDefault(x => x.Email == email);
            if (user == null)
                return Result<bool>.Fail(
                    new Error("USER_NOT_FOUND", "User not found", ErrorType.NotFound)
                );

            var isConfirmed = await _authService.IsEmailConfirmed(email);
            if (isConfirmed)
                return Result<bool>.Fail(
                    new Error("ALREADY_CONFIRMED", "Email is already confirmed", ErrorType.Validation)
                );

            var code = await _authService.GenerateEmailVerificationCode(email);
            await _emailService.SendVerificationCodeEmail(email, code);

            return Result<bool>.Ok(true);
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

            var user = _userRepository.GetAll().FirstOrDefault(x => x.Email == email);

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

            await _authService.SaveRefreshToken(email, newRefreshToken);

            response = new TokensDto
            {
                Token = accessToken,
                RefreshToken = newRefreshToken
            };

            return Result<TokensDto>.Ok(response);
        }
    }
}
