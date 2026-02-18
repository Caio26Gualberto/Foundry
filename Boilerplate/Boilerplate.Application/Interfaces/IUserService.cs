using Boilerplate.Application.Dtos.Users;

namespace Boilerplate.Application.Interfaces
{
    public interface IUserService
    {
        public Task<List<UserDto>> GetAllUSers();
        public Task<List<UserInviteDto>> GetAllInvites();
        public Task<bool> DeleteUser(int id);
        public Task<bool> UpdateUser(int id, UpdateUserDto input);
    }
}
