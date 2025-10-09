using Boilerplate.Application.Interfaces.ICurrentUserContext;

namespace Boilerplate.Infra.Data.Context.Factory
{
    public class DesignTimeCurrentContextService : ICurrentUserContext
    {
        public int? UserId => null;
        public int? TenantId => null;
        public bool IsAuthenticated => false;
        public string Email => string.Empty;
        int ICurrentUserContext.UserId => 0;
    }
}
