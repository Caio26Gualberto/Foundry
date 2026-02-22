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
    public class TenantController : ControllerBase
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
            var tenants = await _tenantService.GetAllTenants();
            return Ok(new BoilerplateResponse<List<TenantDto>>
            {
                IsSuccess = true,
                Data = tenants
            });
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpPatch("{id}")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> UpdateTenant(int id, TenantCreateOrUpdateDto input)
        {
            await _tenantService.Update(id, input.Name, input.Address);
            return NoContent();
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpPost]
        public async Task<ActionResult<BoilerplateResponse<int>>> CreateTenant(TenantCreateOrUpdateDto input)
        {
            await _tenantService.Create(input.Name, input.Address, input.RegisterInput);
            return Created();
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpDelete]
        public async Task<ActionResult<BoilerplateResponse<bool>>> DeleteTenant()
        {
            await _tenantService.Delete();
            return NoContent();
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpPost("invite")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> InviteUser([FromBody] TenantInvitationDto dto)
        {
            var result = await _tenantService.InviteUserToTenantAsync(dto.TenantId, dto.Email);
            if (result)
                return Ok(new BoilerplateResponse<bool>
                {
                    Data = result,
                    IsSuccess = result,
                    Message = "Convite enviado com sucesso"
                });
            return StatusCode(500, new BoilerplateResponse<bool> { IsSuccess = false, Message = "Algo deu errado, tente novamente mais tarde" });
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpPost("impersonate")]
        public async Task<ActionResult<BoilerplateResponse<TokensDto>>> ImpersonateTenant(TenantImpersonateDto input)
        {
            var tokens = await _tenantService.ImpersonateTenantByUser(input.UserId, input.TenantId);
            return Ok(new BoilerplateResponse<TokensDto>
            {
                IsSuccess = true,
                Message = $"Impersonando TenantId {input.TenantId}",
                Data = tokens
            });
        }
    }
}
