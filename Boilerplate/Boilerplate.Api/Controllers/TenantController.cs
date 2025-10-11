using Boilerplate.Api.ApiResponse;
using Boilerplate.Application.Dtos.Tenants;
using Boilerplate.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Boilerplate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        public TenantController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpPost]
        public async Task<ActionResult<BoilerplateResponse<List<TenantDto>>>> GetAll()
        {
            var tenants = await _tenantService.GetAllTenants();
            return Ok(tenants);
        }

        [HttpPost("invite")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> InviteUser([FromBody] TenantInvitationInput dto)
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
    }
}
