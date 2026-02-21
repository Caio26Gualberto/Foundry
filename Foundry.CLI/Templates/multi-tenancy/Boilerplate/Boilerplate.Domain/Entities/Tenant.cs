using Boilerplate.Domain.Models;

namespace Boilerplate.Domain.Entities
{
    public class Tenant : EntityBase
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public Address Address { get; set; }
        public List<User> Users { get; set; } = new();
    }
}
