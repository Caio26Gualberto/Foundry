namespace Boilerplate.Domain.Entities
{
    public class User : EntityBase
    {
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
