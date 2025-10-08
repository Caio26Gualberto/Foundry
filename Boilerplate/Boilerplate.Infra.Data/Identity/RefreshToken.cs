using System.ComponentModel.DataAnnotations;

namespace Boilerplate.Infra.Data.Identity
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
