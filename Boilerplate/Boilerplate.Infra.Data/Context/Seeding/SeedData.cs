using Boilerplate.Domain.Constants;
using Boilerplate.Domain.Entities;
using Boilerplate.Infra.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Boilerplate.Infra.Data.Context.Seeding
{
    public class SeedData
    {
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BoilerplateDbContext _context;
        public SeedData(RoleManager<IdentityRole<int>> roleManager, UserManager<ApplicationUser> userManager, BoilerplateDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task SeedAsync()
        {
            var roles = new[] { Roles.AdminGlobal, Roles.GlobalManager, Roles.TenantAdmin, Roles.User, Roles.Guest };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole<int>(role));
            }

            if (!await _context.Tenants.AnyAsync(t => t.Name == "Boilerplate"))
            {
                var tenant = new Tenant
                {
                    Name = "Boilerplate",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();
            }

            var tenantDb = await _context.Tenants.FirstAsync(t => t.Name == "Boilerplate");

            var adminEmail = "admin@boilerplate.com";
            if (await _userManager.FindByEmailAsync(adminEmail) == null)
            {
                var domainUser = new User
                {
                    Name = "Admin123",
                    Email = adminEmail,
                    TenantId = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Users.Add(domainUser);
                await _context.SaveChangesAsync();

                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    DomainUserId = domainUser.Id
                };

                await _userManager.CreateAsync(adminUser, "Admin123");
                await _userManager.AddToRoleAsync(adminUser, Roles.AdminGlobal);
            }
        }
    }
}
