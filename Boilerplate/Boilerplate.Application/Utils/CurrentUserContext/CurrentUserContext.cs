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

        public int UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
                return userIdClaim != null ? int.Parse(userIdClaim) : 0;
            }
        }

        public int? TenantId
        {
            get
            {
                var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenantId")?.Value;
                return tenantClaim != null ? int.Parse(tenantClaim) : null;
            }
        }

        public string Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    }
}
