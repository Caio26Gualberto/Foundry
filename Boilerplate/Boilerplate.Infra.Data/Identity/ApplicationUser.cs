using Boilerplate.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Boilerplate.Infra.Data.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        public int TenantId { get; set; } //<-- Se o usuário não quer multitenancy, pode remover isso
        public Tenant Tenant { get; set; }  //<-- Se o usuário não quer multitenancy, pode remover isso
        public int DomainUserId { get; set; }
        public User User { get; set; }
    }
}
