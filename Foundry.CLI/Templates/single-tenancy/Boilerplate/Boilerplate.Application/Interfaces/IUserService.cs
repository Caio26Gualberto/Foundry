using Boilerplate.Application.Common.Results;
using Boilerplate.Application.Dtos.Users;

namespace Boilerplate.Application.Interfaces
{
    public interface IUserService
    {
        public Task<Result<List<UserDto>>> GetAllUSers();
        public Task<Result<bool>> DeleteUser(int id);
        public Task<Result<bool>> UpdateUser(int id, UpdateUserDto input);
    }
}
