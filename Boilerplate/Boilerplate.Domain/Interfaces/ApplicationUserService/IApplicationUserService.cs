namespace Boilerplate.Domain.Interfaces.ApplicationUserService
{
    public interface IApplicationUserService
    {
        public Task<IList<string>> GetUserRole(int userId);
    }
}
