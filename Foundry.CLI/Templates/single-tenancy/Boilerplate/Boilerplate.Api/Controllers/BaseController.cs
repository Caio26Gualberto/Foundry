using Boilerplate.Api.ApiResponse;
using Boilerplate.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace Boilerplate.Api.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected ActionResult MapError(Error error)
        {
            var response = new BoilerplateResponse<object>
            {
                IsSuccess = false,
                Message = error.Message,
            };

            return error.Type switch
            {
                ErrorType.Validation => BadRequest(response),
                ErrorType.NotFound => NotFound(response),
                ErrorType.Unauthorized => Unauthorized(response),
                ErrorType.Forbidden => StatusCode(403, response),
                ErrorType.Conflict => Conflict(response),
                _ => StatusCode(500, response)
            };
        }
    }
}
