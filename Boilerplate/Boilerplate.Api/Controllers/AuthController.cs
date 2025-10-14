using Boilerplate.Api.ApiResponse;
using Boilerplate.Application.Dtos.Auth;
using Boilerplate.Application.DTOs.Auth;
using Boilerplate.Application.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Boilerplate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthAppService _authAppService;
        public AuthController(AuthAppService authAppService)
        {
            _authAppService = authAppService;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<BoilerplateResponse<LoginResponseDto>>> Login(LoginInputDto input)
        {
            var resultLogin = await _authAppService.Authenticate(input.Email, input.Password);

            return new BoilerplateResponse<LoginResponseDto>()
            {
                IsSuccess = true,
                Data = new LoginResponseDto
                {
                    Tokens = resultLogin.Tokens != null ? new TokensDto
                    {
                        Token = resultLogin.Tokens.Token,
                        RefreshToken = resultLogin.Tokens.RefreshToken
                    } : null
                }
            };
        }

        [HttpPost("Register")]
        public async Task<ActionResult<BoilerplateResponse<RegisterResponseDto>>> Register(RegisterInputDto input)
        {
            var result = await _authAppService.Register(input);

            return Ok(new BoilerplateResponse<RegisterResponseDto>()
            {
                Data = new RegisterResponseDto
                {
                    Result = result.Result,
                    Message = result.Message,
                    UserId = result.UserId
                },
                IsSuccess = result.Result,
                Message = result.Message
            });
        }

        [HttpGet("Logout")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> Logout()
        {
            var result = await _authAppService.Logout();

            if (result)
                return Ok(new BoilerplateResponse<bool>()
                {
                    Data = result,
                    IsSuccess = result,
                    Message = "Deslogado com sucesso"
                });

            return StatusCode(500, new BoilerplateResponse<bool> { IsSuccess = false, Message = "Algo deu errado, tente novamente mais tarde" });
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<BoilerplateResponse<TokensDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {

            var tokens = await _authAppService.RefreshTokens(request.RefreshToken);

            if (!string.IsNullOrEmpty(tokens.Token))
            {
                return Ok(new BoilerplateResponse<TokensDto>
                {
                    Data = new TokensDto
                    {
                        Token = tokens.Token,
                        RefreshToken = tokens.RefreshToken
                    },
                    IsSuccess = true,
                    Message = "Tokens renovados com sucesso"
                });
            }

            return Unauthorized(new BoilerplateResponse<TokensDto>
            {
                IsSuccess = false,
                Message = "Refresh token inválido ou expirado"
            });
        }

        [HttpPost("ForgotPassword")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> ForgotPassword(ForgotPasswordRequestDto request)
        {
            var result = await _authAppService.ForgotPassword(request);

            return Ok(new BoilerplateResponse<bool>
            {
                Data = result.IsSuccess,
                IsSuccess = result.IsSuccess,
                Message = result.Message
            });
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> ResetPassword(ResetPasswordRequestDto request)
        {
            var result = await _authAppService.ResetPassword(request);

            return Ok(new BoilerplateResponse<bool>
            {
                Data = result.IsSuccess,
                IsSuccess = result.IsSuccess,
                Message = result.Message
            });
        }

        [HttpGet("confirm-email")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> ConfirmEmail(string userId, string token)
        {
            var result = await _authAppService.ConfirmEmail(userId, token);

            return Ok(new BoilerplateResponse<bool>
            {
                IsSuccess = result,
                Message = result ? "Email confirmado com sucesso." : "Falha ao confirmar o email.",
            });
        }

        [HttpPost("validate-invitation-token")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> ValidateToken(ValidateInvitationTokenInputDto input)
        {
            var isValid = await _authAppService.ValidateInviteToken(input);
            return Ok(new BoilerplateResponse<bool>
            {
                IsSuccess = isValid,
                Data = isValid,
            });
        }

        [HttpPost("acceptTenantInvite")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> AcceptInvite(AcceptTenantInvitationInputDto input)
        {
            var isAccepted = await _authAppService.AcceptTenantInvite(input);
            return Ok(new BoilerplateResponse<bool>
            {
                IsSuccess = isAccepted,
                Data = isAccepted
            });
        }
    }
}
