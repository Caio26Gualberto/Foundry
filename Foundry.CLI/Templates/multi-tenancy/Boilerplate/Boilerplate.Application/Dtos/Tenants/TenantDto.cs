using Boilerplate.Application.Dtos.Users;
using Boilerplate.Domain.Models;

namespace Boilerplate.Application.Dtos.Tenants
{
    public class TenantDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public List<UserDto> Users { get; set; }
    }
}
