using Boilerplate.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Boilerplate.Infra.Data.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        public int DomainUserId { get; set; }
        public User User { get; set; } = new User();
        public bool IsNeededChangePassword { get; set; } = false;
        public string? EmailVerificationCode { get; set; }
        public DateTime? EmailVerificationCodeExpiry { get; set; }
    }
}
