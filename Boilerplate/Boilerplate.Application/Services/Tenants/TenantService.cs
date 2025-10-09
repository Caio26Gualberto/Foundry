using Boilerplate.Application.Interfaces;
using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Interfaces.Repositories;
using System.Security.Cryptography;

namespace Boilerplate.Application.Services.Tenants
{
    public class TenantService : ITenantService
    {
        private readonly IRepository<Tenant> _repository;
        private readonly IRepository<TenantInvitation> _tenantInvitationRepository;
        private readonly IEmailService _emailService;

        public TenantService(IRepository<Tenant> repository, IRepository<TenantInvitation> tenantInvitationRepository, IEmailService emailService)
        {
            _repository = repository;
            _tenantInvitationRepository = tenantInvitationRepository;
            _emailService = emailService;
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

            var emailSent = await _emailService.SendTenantInvitationEmailAsync(userEmail, tenant.Name, token);
            if (!emailSent)
                throw new Exception("Failed to send invitation email");

            return true;
        }
    }
}
