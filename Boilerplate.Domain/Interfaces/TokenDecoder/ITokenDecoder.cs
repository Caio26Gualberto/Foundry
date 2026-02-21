using System.Security.Claims;

namespace Boilerplate.Domain.Interfaces.TokenDecoder
{
    public interface ITokenDecoder
    {
        ClaimsPrincipal DecodeToken(string token);
    }
}
