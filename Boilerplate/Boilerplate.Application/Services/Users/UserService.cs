using Boilerplate.Application.Dtos.Users;
using Boilerplate.Application.Interfaces;
using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Interfaces.ApplicationUserService;
using Boilerplate.Domain.Interfaces.Repositories;

namespace Boilerplate.Application.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly IApplicationUserService _applicationUserService;
        public UserService(IRepository<User> repository, IApplicationUserService applicationUserService)
        {
            _repository = repository;
            _applicationUserService = applicationUserService;
        }

        public async Task<List<UserDto>> GetAllUSers()
        {
            var users = new List<UserDto>();
            var tenantUsers = _repository.GetAll().ToList();
            foreach (var tenantUser in tenantUsers) 
            {
                users.Add(new UserDto
                {
                    Id = tenantUser.Id,
                    Email = tenantUser.Email,
                    Name = tenantUser.Name,
                    Roles = await _applicationUserService.GetUserRole(tenantUser.Id)
                });
            }
            return users;
        }
    }
}
