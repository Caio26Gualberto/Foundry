using Boilerplate.Domain.Interfaces.TokenDecoder;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Boilerplate.Infra.Data.Identity
{
    public class TokenDecoder : ITokenDecoder
    {
        public ClaimsPrincipal DecodeToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var identity = new ClaimsIdentity(jwt.Claims, "Token");
            return new ClaimsPrincipal(identity);
        }
    }
}
