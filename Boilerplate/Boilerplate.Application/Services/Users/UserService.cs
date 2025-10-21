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
        private readonly IRepository<TenantInvitation> _tenantInvitationRepository;
        private readonly IApplicationUserService _applicationUserService;
        public UserService(IRepository<User> repository, IRepository<TenantInvitation> tenantInvitationRepository, IApplicationUserService applicationUserService)
        {
            _repository = repository;
            _tenantInvitationRepository = tenantInvitationRepository;
            _applicationUserService = applicationUserService;
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
    }
}
