using Boilerplate.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Boilerplate.Infra.Data.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        public int? TenantId { get; set; }
        public Tenant? Tenant { get; set; } 
        public int DomainUserId { get; set; }
        public User User { get; set; }
    }
}
