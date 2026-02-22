using Boilerplate.Domain.Models;

namespace Boilerplate.Domain.Entities
{
    public class Tenant : EntityBase
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Address Address { get; set; } = new Address();
        public List<User> Users { get; set; } = new();
    }
}
