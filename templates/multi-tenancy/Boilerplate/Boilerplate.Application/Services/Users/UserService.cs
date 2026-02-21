using Boilerplate.Application.Dtos.Users;
using Boilerplate.Application.Interfaces;
using Boilerplate.Application.Interfaces.ICurrentUserContext;
using Boilerplate.Application.Utils.StaticUtils;
using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Interfaces.ApplicationUserService;
using Boilerplate.Domain.Interfaces.Repositories;
using Boilerplate.Domain.Models;
using System.Xml.Linq;

namespace Boilerplate.Application.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly IRepository<TenantInvitation> _tenantInvitationRepository;
        private readonly IApplicationUserService _applicationUserService;
        private readonly ICurrentUserContext _currentUserContext;
        public UserService(IRepository<User> repository, IRepository<TenantInvitation> tenantInvitationRepository, IApplicationUserService applicationUserService, 
            ICurrentUserContext currentUserContext)
        {
            _repository = repository;
            _tenantInvitationRepository = tenantInvitationRepository;
            _applicationUserService = applicationUserService;
            _currentUserContext = currentUserContext;
        }

        public async Task<bool> DeleteUser(int id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                throw new Exception("User not found");

            await _repository.SoftDelete(user);
            return true;
        }

        public async Task<List<UserInviteDto>> GetAllInvites()
        {
            var invites = _tenantInvitationRepository.GetAll();
            return invites.Select(x => new UserInviteDto
            {
                Email = x.Email,
                ExpirationTime = x.ExpiresAt,
                SendedAt = x.CreatedAt,
                Status = x.Status.ToString()
            }).ToList();
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

        public async Task<bool> UpdateUser(int id, UpdateUserDto input)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                throw new NullReferenceException("Usuário não encontrado para atualizar");

            var result = await _applicationUserService.UpdateUserRoles(id, input.Roles);
            if (result == false)
                return false;

            BoilerplateStaticUtils.ApplyChanges<User, UpdateUserDto>(user, input);
            await _repository.UpdateAsync(user);
            return true;
        }
    }
}
