using Boilerplate.Api.ApiResponse;
using Boilerplate.Application.Dtos.SystemNotification;
using Boilerplate.Application.Interfaces;
using Boilerplate.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Boilerplate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SystemNotificationController : ControllerBase
    {
        private readonly ISystemNotificationService _notificationService;
        public SystemNotificationController(ISystemNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<BoilerplateResponse<List<SystemNotificationDto>>>> GetAll()
        {
            var notifications = await _notificationService.GetAllNotifications();
            return Ok(new BoilerplateResponse<List<SystemNotificationDto>>
            {
                IsSuccess = true,
                Data = notifications
            });
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.GlobalManager},{Roles.AdminGlobal}")]
        public async Task<ActionResult<BoilerplateResponse<SystemNotificationDto>>> Create(CreateSystemNotificationDto input)
        {
            var notification = await _notificationService.CreateSystemNotification(input);
            return Ok(new BoilerplateResponse<SystemNotificationDto>
            {
                IsSuccess = true,
                Data = notification
            });
        }

        [HttpPatch("MarkAsRead/{id}")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> MarkAsRead(int id, [FromBody] MarkAsReadDto input)
        {
            var result = await _notificationService.MarkNotificationAsRead(id, input);
            return Ok(new BoilerplateResponse<bool>
            {
                IsSuccess = true,
                Data = result
            });
        }

        [HttpPost("ClearAllMessages")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> ClearAllMessages(ClearAllMessagesDto input)
        {
            var result = await _notificationService.DeleteAllMessages(input);
            return Ok(new BoilerplateResponse<bool>
            {
                IsSuccess = true,
                Data = result
            });
        }
    }
}
