using Boilerplate.Api.ApiResponse;
using Boilerplate.Api.RateLimiting;
using Boilerplate.Api.RateLimiting.Policies;
using Boilerplate.Application.Dtos.Auth;
using Boilerplate.Application.DTOs.Auth;
using Boilerplate.Application.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Boilerplate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly AuthAppService _authAppService;
        public AuthController(AuthAppService authAppService)
        {
            _authAppService = authAppService;
        }

        [RateLimit(typeof(LoginRateLimitPolicy))]
        [HttpPost("Login")]
        public async Task<ActionResult<BoilerplateResponse<LoginResponseDto>>> Login(LoginInputDto input)
        {
            var result = await _authAppService.Authenticate(input.Email, input.Password);

            if (result.IsFailure)
                return MapError(result.Error);

            return new BoilerplateResponse<LoginResponseDto>()
            {
                IsSuccess = result.IsSuccess,
                Data = new LoginResponseDto
                {
                    Tokens = result.Data.Tokens != null ? new TokensDto
                    {
                        Token = result.Data.Tokens.Token,
                        RefreshToken = result.Data.Tokens.RefreshToken
                    } : null,
                    IsNeededChangePassword = result.Data.IsNeededChangePassword
                }
            };
        }

        [RateLimit(typeof(LoginRateLimitPolicy))]
        [HttpPost("Register")]
        public async Task<ActionResult<BoilerplateResponse<RegisterResponseDto>>> Register(RegisterInputDto input)
        {
            var result = await _authAppService.Register(input);

            if (result.IsFailure)
                return MapError(result.Error);

            return Ok(new BoilerplateResponse<RegisterResponseDto>()
            {
                Data = new RegisterResponseDto
                {
                    Result = result.Data.Result,
                    Message = result.Data.Message,
                    UserId = result.Data.UserId
                },
                IsSuccess = result.Data.Result,
                Message = result.Data.Message
            });
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
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

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpPost("RefreshToken")]
        public async Task<ActionResult<BoilerplateResponse<TokensDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {

            var result = await _authAppService.RefreshTokens(request.RefreshToken);

            if (result.IsFailure)
                return MapError(result.Error);

            return Ok(new BoilerplateResponse<TokensDto>
            {
                IsSuccess = true,
                Message = "Tokens successfully renewed.",
                Data = new TokensDto
                {
                    Token = result.Data.Token,
                    RefreshToken = result.Data.RefreshToken
                }
            });
        }

        [RateLimit(typeof(LoginRateLimitPolicy))]
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

        [RateLimit(typeof(LoginRateLimitPolicy))]
        [HttpPost("ResetPassword")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> ResetPassword(ResetPasswordRequestDto request)
        {
            var result = await _authAppService.ResetPassword(request);

            if (result.IsFailure)
                return MapError(result.Error);

            return Ok(new BoilerplateResponse<bool>
            {
                Data = result.IsSuccess,
                IsSuccess = result.IsSuccess,
                Message = result.Data.Message
            });
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpPost("ChangePassword")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> ChangePassword(ChangePasswordDto request)
        {
            var result = await _authAppService.ChangePassword(request);
            return Ok(new BoilerplateResponse<bool>
            {
                IsSuccess = result,
                Data = result
            });
        }

        [RateLimit(typeof(LoginRateLimitPolicy))]
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
    }
}
