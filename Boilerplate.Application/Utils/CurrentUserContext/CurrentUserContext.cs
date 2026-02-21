using Boilerplate.Application.Interfaces.ICurrentUserContext;
using Boilerplate.Domain.Interfaces.Authenticate;
using Boilerplate.Domain.Interfaces.TokenDecoder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Boilerplate.Application.Utils.CurrentUserContext
{
    public class CurrentUserContext : ICurrentUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenDecoder _tokenDecoder;

        public CurrentUserContext(IHttpContextAccessor httpContextAccessor, ITokenDecoder tokenDecoder)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenDecoder = tokenDecoder;
        }

        private ClaimsPrincipal ActiveUser
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return new ClaimsPrincipal();

                var impersonationToken = httpContext.Request.Headers["Boilerplate_impersonated_token"].FirstOrDefault();

                if (!string.IsNullOrEmpty(impersonationToken))
                {
                    var principal = _tokenDecoder.DecodeToken(impersonationToken);
                    if (principal == null)
                    {
                        _httpContextAccessor.HttpContext.Response.StatusCode = 401;
                        return new ClaimsPrincipal();
                    }
                }

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

        public int? TenantId
        {
            get
            {
                var tenantClaim = ActiveUser.FindFirst("tenantId")?.Value;
                return !string.IsNullOrEmpty(tenantClaim) ? int.Parse(tenantClaim) : null;
            }
        }

        public string Email => ActiveUser.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        public bool IsAuthenticated => ActiveUser.Identity?.IsAuthenticated ?? false;

        public int? ImpersonatedBy
        {
            get
            {
                var claim = ActiveUser.FindFirst("impersonatedBy")?.Value;
                return claim != null ? int.Parse(claim) : null;
            }
        }

        public bool IsImpersonating => ImpersonatedBy.HasValue;
    }

}
