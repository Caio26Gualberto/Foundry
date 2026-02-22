using Boilerplate.Domain.Enums;

namespace Boilerplate.Application.Dtos.Users
{
    public class UserInviteDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SendedAt { get; set; }
        public DateTime ExpirationTime { get; set; }
    }
}
