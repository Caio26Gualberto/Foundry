using Boilerplate.Api.ApiResponse;
using Boilerplate.Api.RateLimiting;
using Boilerplate.Api.RateLimiting.Policies;
using Boilerplate.Application.Dtos.Tenants;
using Boilerplate.Application.DTOs.Auth;
using Boilerplate.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Boilerplate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TenantController : BaseController
    {
        private readonly ITenantService _tenantService;
        public TenantController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpGet]
        public async Task<ActionResult<BoilerplateResponse<List<TenantDto>>>> GetAll()
        {
            var result = await _tenantService.GetAllTenants();

            if (result.IsFailure)
                return MapError(result.Error);

            return Ok(new BoilerplateResponse<List<TenantDto>>
            {
                IsSuccess = result.IsSuccess,
                Data = result.Data,
            });
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpPatch("{id}")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> UpdateTenant(int id, TenantCreateOrUpdateDto input)
        {
            var result = await _tenantService.Update(id, input.Name, input.Address);

            if (result.IsFailure)
                return MapError(result.Error);

            return NoContent();
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpPost]
        public async Task<ActionResult<BoilerplateResponse<int>>> CreateTenant(TenantCreateOrUpdateDto input)
        {
            var response = await _tenantService.Create(input.Name, input.Address, input.RegisterInput);

            if (response.IsFailure)
                return MapError(response.Error);

            return Created();
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpDelete]
        public async Task<ActionResult<BoilerplateResponse<bool>>> DeleteTenant()
        {
            var result = await _tenantService.Delete();

            if (result.IsFailure)
                return MapError(result.Error);

            return NoContent();
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpPost("invite")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> InviteUser([FromBody] TenantInvitationDto dto)
        {
            var result = await _tenantService.InviteUserToTenantAsync(dto.TenantId, dto.Email);

            if (result.IsFailure)
                return MapError(result.Error);

            return Ok(new BoilerplateResponse<bool>
            {
                IsSuccess = true,
                Message = "Invitation sent successfully",
                Data = result.Data
            });
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpPost("impersonate")]
        public async Task<ActionResult<BoilerplateResponse<TokensDto>>> ImpersonateTenant(TenantImpersonateDto input)
        {
            var result = await _tenantService.ImpersonateTenantByUser(input.UserId, input.TenantId);

            if (result.IsFailure)
                return MapError(result.Error);

            return Ok(new BoilerplateResponse<TokensDto>
            {
                IsSuccess = true,
                Message = $"Impersonating tenantId {input.TenantId}",
                Data = result.Data
            });
        }
    }
}
