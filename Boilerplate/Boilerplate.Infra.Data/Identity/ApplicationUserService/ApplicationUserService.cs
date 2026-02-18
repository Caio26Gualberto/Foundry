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

        public async Task<bool> UpdateUserRoles(int userId, List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var currentRoles = await _userManager.GetRolesAsync(user);

            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return false;

            var addResult = await _userManager.AddToRolesAsync(user, roles);
            if (!addResult.Succeeded)
                return false;

            return true;
        }

    }
}
