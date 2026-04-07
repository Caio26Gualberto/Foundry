using Boilerplate.Api.ApiResponse;
using Boilerplate.Api.RateLimiting;
using Boilerplate.Api.RateLimiting.Policies;
using Boilerplate.Application.Dtos.Users;
using Boilerplate.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Boilerplate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpGet]
        public async Task<ActionResult<BoilerplateResponse<List<UserDto>>>> GetAll()
        {
            var result = await _userService.GetAllUSers();

            if (result.IsFailure)
                return MapError(result.Error);

            return Ok(new BoilerplateResponse<List<UserDto>>
            {
                IsSuccess = true,
                Data = result.Data
            });
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpDelete("{id}")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> Delete(int id)
        {
            var result = await _userService.DeleteUser(id);

            if (result.IsFailure)
                return MapError(result.Error);

            return NoContent();
        }

        [RateLimit(typeof(UserRateLimitPolicy))]
        [HttpPatch("{id}")]
        public async Task<ActionResult<BoilerplateResponse<bool>>> Update(int id, UpdateUserDto input)
        {
            var result = await _userService.UpdateUser(id, input);

            if (result.IsFailure)
                return MapError(result.Error);

            return NoContent();
        }
    }
}
