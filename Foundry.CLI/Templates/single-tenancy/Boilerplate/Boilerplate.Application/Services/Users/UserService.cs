using Boilerplate.Application.Common.Results;
using Boilerplate.Application.Dtos.Users;
using Boilerplate.Application.Interfaces;
using Boilerplate.Application.Interfaces.ICurrentUserContext;
using Boilerplate.Application.Utils.StaticUtils;
using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Interfaces.ApplicationUserService;
using Boilerplate.Domain.Interfaces.Repositories;
using Boilerplate.Domain.Models;

namespace Boilerplate.Application.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly IApplicationUserService _applicationUserService;
        private readonly ICurrentUserContext _currentUserContext;
        public UserService(IRepository<User> repository, IApplicationUserService applicationUserService, 
            ICurrentUserContext currentUserContext)
        {
            _repository = repository;
            _applicationUserService = applicationUserService;
            _currentUserContext = currentUserContext;
        }

        public async Task<Result<bool>> DeleteUser(int id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                return Result<bool>.Fail(
                    new Error("USER_NOT_FOUND", "User not found", ErrorType.NotFound)
                );

            await _repository.SoftDelete(user);
            return Result<bool>.Ok(true);
        }

        public async Task<Result<List<UserDto>>> GetAllUSers()
        {
            var users = new List<UserDto>();
            var allUsers = _repository.GetAll().ToList();
            foreach (var u in allUsers) 
            {
                users.Add(new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Name = u.Name,
                    Roles = await _applicationUserService.GetUserRole(u.Id)
                });
            }
            return Result<List<UserDto>>.Ok(users);
        }

        public async Task<Result<bool>> UpdateUser(int id, UpdateUserDto input)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                return Result<bool>.Fail(
                    new Error("USER_NOT_FOUND", "User not found", ErrorType.NotFound)
                );

            var result = await _applicationUserService.UpdateUserRoles(id, input.Roles);

            if (result == false)
                return Result<bool>.Fail(
                    new Error("ROLE_UPDATE_FAILED", "Failed to update user roles", ErrorType.Unexpected)
                );

            BoilerplateStaticUtils.ApplyChanges<User, UpdateUserDto>(user, input);
            await _repository.UpdateAsync(user);
            return Result<bool>.Ok(true);
        }
    }
}
