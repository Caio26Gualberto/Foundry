using Boilerplate.Application.Dtos.Users;

namespace Boilerplate.Application.Interfaces
{
    public interface IUserService
    {
        public Task<List<UserDto>> GetAllUSers();
        public Task<List<UserInviteDto>> GetAllInvites();
    }
}
