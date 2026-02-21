using Boilerplate.Application.Dtos.Users;

namespace Boilerplate.Application.Interfaces
{
    public interface IUserService
    {
        public Task<List<UserDto>> GetAllUSers();
        public Task<bool> DeleteUser(int id);
        public Task<bool> UpdateUser(int id, UpdateUserDto input);
    }
}
