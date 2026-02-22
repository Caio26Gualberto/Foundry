using Boilerplate.Application.Dtos.Users;
using Boilerplate.Domain.Models;

namespace Boilerplate.Application.Dtos.Tenants
{
    public class TenantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;    
        public Address Address { get; set; } = new Address();
        public List<UserDto> Users { get; set; } = new List<UserDto>();
    }
}
