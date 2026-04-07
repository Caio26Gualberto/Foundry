using Boilerplate.Api.ApiResponse;
using Boilerplate.Api.RateLimiting;
using Boilerplate.Api.RateLimiting.Policies;
using Boilerplate.Application.Dtos.SystemNotification;
using Boilerplate.Application.Dtos.Tenants;
using Boilerplate.Application.Interfaces;
using Boilerplate.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Boilerplate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SystemNotificationController : BaseController
    {
        private readonly ISystemNotificationService _notificationService;
        public SystemNotificationController(ISystemNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpGet]
        public async Task<ActionResult<BoilerplateResponse<List<SystemNotificationDto>>>> GetAll()
        {
            var result = await _notificationService.GetAllNotifications();

            if (result.IsFailure)
                return MapError(result.Error);

            return Ok(new BoilerplateResponse<List<SystemNotificationDto>>
            {
                IsSuccess = result.IsSuccess,
                Data = result.Data
            });
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpPost]
        [Authorize(Roles = $"{Roles.TenantAdmin},{Roles.GlobalManager},{Roles.AdminGlobal}")]
        public async Task<ActionResult<BoilerplateResponse<SystemNotificationDto>>> Create(CreateSystemNotificationDto input)
        {
            var result = await _notificationService.CreateSystemNotification(input);

            if (result.IsFailure)
                return MapError(result.Error);

            return Ok(new BoilerplateResponse<SystemNotificationDto>
            {
                IsSuccess = result.IsSuccess,
                Data = result.Data
            });
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpGet("GetNotificationsCreatedByTenant")]
        public async Task<ActionResult<BoilerplateResponse<List<TenantNotificationDto>>>> GetNotificationsCreatedByTenant()
        {
            var result = await _notificationService.GetNotificationByTenant();

            if (result.IsFailure)
                return MapError(result.Error);

            return Ok(new BoilerplateResponse<List<TenantNotificationDto>>
            {
                IsSuccess = result.IsSuccess,
                Data = result.Data
            });
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpPatch("MarkAsRead/{id}")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> MarkAsRead(int id, [FromBody] MarkAsReadDto input)
        {
            var result = await _notificationService.MarkNotificationAsRead(id, input);

            if (result.IsFailure)
                return MapError(result.Error);

            return Ok(new BoilerplateResponse<bool>
            {
                IsSuccess = result.IsSuccess,
                Data = result.Data
            });
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpPost("ClearAllMessages")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> ClearAllMessages(ClearAllMessagesDto input)
        {
            var result = await _notificationService.DeleteAllMessages(input);

            if (result.IsFailure)
                return MapError(result.Error);

            return Ok(new BoilerplateResponse<bool>
            {
                IsSuccess = result.IsSuccess,
                Data = result.Data
            });
        }
    }
}
