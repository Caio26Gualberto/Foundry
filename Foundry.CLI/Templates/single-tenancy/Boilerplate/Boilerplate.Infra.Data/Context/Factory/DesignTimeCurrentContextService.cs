using Boilerplate.Application.Interfaces.ICurrentUserContext;

namespace Boilerplate.Infra.Data.Context.Factory
{
    public class DesignTimeCurrentContextService : ICurrentUserContext
    {
        public int UserId => 0;
        public bool IsAuthenticated => false;
        public string Email => string.Empty;
    }
}
