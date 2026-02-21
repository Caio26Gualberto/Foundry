using Boilerplate.Application.Interfaces.ICurrentUserContext;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Boilerplate.Application.Utils.CurrentUserContext
{
    public class CurrentUserContext : ICurrentUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal ActiveUser
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return new ClaimsPrincipal();

                return httpContext.User;
            }
        }

        public int UserId
        {
            get
            {
                var userIdClaim = ActiveUser.FindFirst("userId")?.Value;
                return userIdClaim != null ? int.Parse(userIdClaim) : 0;
            }
        }

        public string Email => ActiveUser.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        public bool IsAuthenticated => ActiveUser.Identity?.IsAuthenticated ?? false;
    }
}
