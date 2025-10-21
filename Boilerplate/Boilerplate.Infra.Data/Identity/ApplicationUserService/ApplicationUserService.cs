using Boilerplate.Domain.Interfaces.ApplicationUserService;
using Microsoft.AspNetCore.Identity;

namespace Boilerplate.Infra.Data.Identity.ApplicationUserService
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ApplicationUserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IList<string>> GetUserRole(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new Exception("User not found");

            return await _userManager.GetRolesAsync(user);
        }
    }
}
