using Boilerplate.Application.Dtos.Tenants;
using Boilerplate.Application.Dtos.Users;
using Boilerplate.Application.DTOs.Auth;
using Boilerplate.Application.Interfaces;
using Boilerplate.Application.Interfaces.ICurrentUserContext;
using Boilerplate.Application.Utils.StaticUtils;
using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Interfaces.Authenticate;
using Boilerplate.Domain.Interfaces.Repositories;
using Boilerplate.Domain.Models;
using System.Security.Cryptography;

namespace Boilerplate.Application.Services.Tenants
{
    public class TenantService : ITenantService
    {
        private readonly IRepository<Tenant> _repository;
        private readonly IRepository<TenantInvitation> _tenantInvitationRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Entity1> _hahahaha;
        private readonly IEmailService _emailService;
        private readonly IAuthenticateService _authenticateService;
        private readonly ICurrentUserContext _currentUserContext;

        public TenantService(IRepository<Tenant> repository, IRepository<TenantInvitation> tenantInvitationRepository, IRepository<User> userRepository,
            IRepository<Entity1> teste, IEmailService emailService, IAuthenticateService authenticateService, ICurrentUserContext currentUserContext)
        {
            _repository = repository;
            _tenantInvitationRepository = tenantInvitationRepository;
            _userRepository = userRepository;
            _hahahaha = teste;
            _emailService = emailService;
            _authenticateService = authenticateService;
            _currentUserContext = currentUserContext;
        }

        public async Task<int> Create(string name, Address address, RegisterInputDto registerDto)
        {
            Tenant tenant = new Tenant
            {
                Name = name,
                Address = address
            };
            
            await _repository.AddAsync(tenant);

            await _authenticateService.RegisterTenantAdmin(registerDto.Email, registerDto.Password, registerDto.Nickname, tenant.Id);
            return tenant.Id;
        }

        public async Task<bool> Delete()
        {
            var tenant = await _repository.GetByIdAsync((int)_currentUserContext.TenantId!);
            if (tenant == null)
                throw new NullReferenceException("Tenant não encontrada para exclusão");

            await _repository.SoftDelete(tenant);
            return true;
        }

        public async Task<List<TenantDto>> GetAllTenants()
        {
            var tenants = _repository.GetAll(t => t.Users).ToList();
            return tenants.Select(x => new TenantDto
            {
                Address = x.Address,
                Id = x.Id,
                Name = x.Name,
                Users = x.Users.Select(x => new UserDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email
                }).ToList(),
            }).ToList();        
        }

        public async Task<TokensDto> ImpersonateTenantByUser(int userId, int tenantId)
        {
            var adminUser = await _userRepository.GetByIdAsync(_currentUserContext.UserId);
            if (adminUser == null)
                throw new UnauthorizedAccessException("Usuário não encontrado.");

            var targetUser = _userRepository.GetAll(tu => tu.Tenant).FirstOrDefault(x => x.Id == userId);
            if (targetUser == null)
                throw new ArgumentException("Usuário alvo não encontrado.");

            var tenantExists = _repository.GetAll()
                .Any(t => t.Id == tenantId && t.Users.Any(u => u.Id == userId));

            if (!tenantExists)
                throw new ArgumentException("A tenant não existe ou o usuário alvo não pertence a esta tenant.");

            var token = await _authenticateService.GenerateJwtImpersonatorToken(adminUser, targetUser);

            return new TokensDto
            {
                Token = token
            };
        }

        public async Task<bool> InviteUserToTenantAsync(int tenantId, string userEmail)
        {
            var tenant = await _repository.GetByIdAsync(tenantId);
            if (tenant == null)
                throw new NullReferenceException("Tenant not found");

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            await _tenantInvitationRepository.AddAsync(new TenantInvitation
            {
                TenantId = tenantId,
                Email = userEmail,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            });

            var emailSent = await _emailService.SendTenantInvitationEmailAsync(userEmail, tenant, token);
            if (!emailSent)
                throw new Exception("Failed to send invitation email");

            return true;
        }

        public async Task<bool> Update(string name, Address address)
        {
            var tenant = await _repository.GetByIdAsync((int)_currentUserContext.TenantId!);
            if (tenant == null)
                throw new NullReferenceException("Tenant não encontrada para atualizar");

            if (!string.IsNullOrEmpty(name) && tenant.Name != name)            
                tenant.Name = name;
        
            BoilerplateStaticUtils.ApplyChanges<Address, Address>(tenant.Address, address);
            await _repository.UpdateAsync(tenant);
            return true;
        }
    }
}
