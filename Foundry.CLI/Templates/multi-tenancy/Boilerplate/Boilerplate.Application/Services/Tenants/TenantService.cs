using Boilerplate.Application.Common.Results;
using Boilerplate.Application.Dtos.Tenants;
using Boilerplate.Application.Dtos.Users;
using Boilerplate.Application.DTOs.Auth;
using Boilerplate.Application.Interfaces;
using Boilerplate.Application.Interfaces.ICurrentUserContext;
using Boilerplate.Application.Utils.StaticUtils;
using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Interfaces.Authenticate;
using Boilerplate.Domain.Interfaces.Repositories;
using Boilerplate.Domain.Interfaces.Repositories.IUnitOfWork;
using Boilerplate.Domain.Models;
using System.Security.Cryptography;

namespace Boilerplate.Application.Services.Tenants
{
    public class TenantService : ITenantService
    {
        private readonly IRepository<Tenant> _repository;
        private readonly IRepository<TenantInvitation> _tenantInvitationRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IEmailService _emailService;
        private readonly IAuthenticateService _authenticateService;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly IUnitOfWork _unitOfWork;

        public TenantService(IRepository<Tenant> repository, IRepository<TenantInvitation> tenantInvitationRepository, IRepository<User> userRepository,
            IEmailService emailService, IAuthenticateService authenticateService, ICurrentUserContext currentUserContext, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _tenantInvitationRepository = tenantInvitationRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _authenticateService = authenticateService;
            _currentUserContext = currentUserContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Create(string name, Address address, RegisterInputDto registerDto)
        {
            if (string.IsNullOrEmpty(name))
                return Result<int>.Fail(
                    new Error("INVALID_NAME", "The tenant's name is required.", ErrorType.Validation)
                );

            if (string.IsNullOrEmpty(registerDto.Email) || string.IsNullOrEmpty(registerDto.Password) || string.IsNullOrEmpty(registerDto.Nickname))
                return Result<int>.Fail(
                    new Error("INVALID_ADMIN_DATA", "Admin user data is incomplete.", ErrorType.Validation)
                );

            if (string.IsNullOrEmpty(registerDto.Token))
                return Result<int>.Fail(
                    new Error("INVALID_TOKEN", "Token should be provided when creating a tenant.", ErrorType.Validation)
                );

            if (string.IsNullOrEmpty(address.Street) || string.IsNullOrEmpty(address.Number) || string.IsNullOrEmpty(address.City) || string.IsNullOrEmpty(address.State) ||
                string.IsNullOrEmpty(address.Country) || string.IsNullOrEmpty(address.ZipCode))
                return Result<int>.Fail(
                    new Error("INVALID_ADDRESS", "Complete address information is required.", ErrorType.Validation)
                );

            Tenant tenant = new Tenant
            {
                Name = name,
                Address = address
            };
            
            await _repository.AddAsync(tenant);

            await _authenticateService.RegisterTenantAdmin(registerDto.Email, registerDto.Password, registerDto.Nickname, tenant.Id);
            return Result<int>.Ok(tenant.Id);
        }

        public async Task<Result<bool>> Delete()
        {
            var tenant = await _repository.GetByIdAsync((int)_currentUserContext.TenantId!);
            if (tenant == null)
                return Result<bool>.Fail(
                    new Error("TENANT_NOT_FOUND", "Tenant not found for deletion.", ErrorType.NotFound)
                );

            await _repository.SoftDelete(tenant);
            return Result<bool>.Ok(true);
        }

        public async Task<Result<List<TenantDto>>> GetAllTenants()
        {
            var tenants = _repository.GetAll(t => t.Users).ToList();

            var result = tenants.Select(x => new TenantDto
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
            
            return Result<List<TenantDto>>.Ok(result);
        }

        public async Task<Result<TokensDto>> ImpersonateTenantByUser(int userId, int tenantId)
        {
            var adminUser = await _userRepository.GetByIdAsync(_currentUserContext.UserId);
            if (adminUser == null)
                return Result<TokensDto>.Fail(
                    new Error("USER_NOT_FOUND", "Current user not found for impersonation.", ErrorType.NotFound)
                );

            var targetUser = _userRepository.GetAll(tu => tu.Tenant!).FirstOrDefault(x => x.Id == userId);
            if (targetUser == null)
                return Result<TokensDto>.Fail(
                    new Error("TARGET_USER_NOT_FOUND", "Target user not found for impersonation.", ErrorType.NotFound)
                );

            var tenantExists = _repository.GetAll()
                .Any(t => t.Id == tenantId && t.Users.Any(u => u.Id == userId));

            if (!tenantExists)
                return Result<TokensDto>.Fail(
                    new Error("TENANT_OR_USER_MISMATCH", "Tenant does not exist or target user does not belong to this tenant.", ErrorType.Validation)
                );

            var token = await _authenticateService.GenerateJwtImpersonatorToken(adminUser, targetUser);

            return Result<TokensDto>.Ok(new TokensDto { Token = token });
        }

        public async Task<Result<bool>> InviteUserToTenantAsync(int tenantId, string userEmail)
        {
            var tenant = await _repository.GetByIdAsync(tenantId);
            if (tenant == null)
                return Result<bool>.Fail(
                    new Error("TENANT_NOT_FOUND", "Tenant not found for invitation.", ErrorType.NotFound)
                );

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            await _unitOfWork.BeginTransactionAsync();

            await _tenantInvitationRepository.AddAsync(new TenantInvitation
            {
                TenantId = tenantId,
                Email = userEmail,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            });

            var emailSent = await _emailService.SendTenantInvitationEmailAsync(userEmail, tenant, token);
            if (!emailSent)
            {
                await _unitOfWork.RollbackAsync();
                return Result<bool>.Fail(
                    new Error("EMAIL_SENDING_FAILED", "Failed to send invitation email.", ErrorType.Unexpected)
                );
            }

            await _unitOfWork.CommitAsync();

            return Result<bool>.Ok(true);
        }

        public async Task<Result<bool>> Update(int id, string name, Address address)
        {
            var tenantId = _currentUserContext.TenantId ?? id;
            var tenant = await _repository.GetByIdAsync(tenantId);

            if (tenant == null)
                return Result<bool>.Fail(
                    new Error("TENANT_NOT_FOUND", "Tenant not found for update.", ErrorType.NotFound)
                );

            if (!string.IsNullOrEmpty(name) && tenant.Name != name)            
                tenant.Name = name;
        
            BoilerplateStaticUtils.ApplyChanges<Address, Address>(tenant.Address, address);
            await _repository.UpdateAsync(tenant);
            return Result<bool>.Ok(true);
        }
    }
}
