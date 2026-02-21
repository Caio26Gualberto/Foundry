using Boilerplate.Api.ApiResponse;
using Boilerplate.Application.Dtos.Users;
using Boilerplate.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Boilerplate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<BoilerplateResponse<List<UserDto>>>> GetAll()
        {
            var users = await _userService.GetAllUSers();
            return Ok(new BoilerplateResponse<List<UserDto>>
            {
                IsSuccess = true,
                Data = users
            });
        }

        [HttpGet("GetInvites")]
        public async Task<ActionResult<BoilerplateResponse<List<UserInviteDto>>>> GetAllInvites()
        {
            var invites = await _userService.GetAllInvites();
            return Ok(new BoilerplateResponse<List<UserInviteDto>>
            {
                IsSuccess = true,
                Data = invites
            });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> Delete(int id)
        {
            await _userService.DeleteUser(id);
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> Update(int id, UpdateUserDto input)
        {
            await _userService.UpdateUser(id, input);
            return NoContent();
        }
    }
}
