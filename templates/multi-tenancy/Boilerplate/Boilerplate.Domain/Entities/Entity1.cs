namespace Boilerplate.Domain.Entities
{
    public class Entity1 : EntityBase
    {
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }
}
