using Boilerplate.Api.ApiResponse;
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

        [HttpPut("{tenantId}")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> UpdateTenant(int tenantId, [FromBody]TenantCreateOrUpdateDto input)
        {
            await _tenantService.Update(tenantId, input.Name, input.Address);
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<BoilerplateResponse<int>>> CreateTenant(TenantCreateOrUpdateDto input)
        {
            await _tenantService.Create(input.Name, input.Address);
            return Created();
        }

        [HttpDelete("{tenantId}")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> DeleteTenant(int tenantId)
        {
            await _tenantService.Delete(tenantId);
            return NoContent();
        }

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
