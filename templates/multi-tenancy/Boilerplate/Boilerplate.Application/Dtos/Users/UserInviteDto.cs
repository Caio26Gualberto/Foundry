using Boilerplate.Domain.Enums;

namespace Boilerplate.Application.Dtos.Users
{
    public class UserInviteDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }    
        public DateTime SendedAt { get; set; }
        public DateTime ExpirationTime { get; set; }
    }
}
