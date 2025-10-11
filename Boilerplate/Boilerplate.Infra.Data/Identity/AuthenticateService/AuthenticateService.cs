using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Interfaces.Authenticate;
using Boilerplate.Infra.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Boilerplate.Infra.Data.Identity.AuthenticateService
{
    public class AuthenticateService : IAuthenticateService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly BoilerplateDbContext _context;

        public AuthenticateService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, BoilerplateDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        public async Task<bool> Authenticate(string email, string password)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return false;

            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            
            return passwordValid;
        }

        public async Task<(int, string)> Register(string email, string password, string name, int tenantId, string token)
        {
            var invite = await _context.TenantInvitations.FirstOrDefaultAsync(x => x.Email == email);
            if (invite == null || invite.TenantId != tenantId || invite.Token != token)
                return (0, string.Empty);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    Email = email,
                    Name = name,
                    TenantId = tenantId
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var applicationUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    DomainUserId = user.Id,
                    TenantId = tenantId
                };

                var result = await _userManager.CreateAsync(applicationUser, password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return (0, string.Empty);
                }

                invite.IsDeleted = true;
                _context.TenantInvitations.Update(invite);
                await transaction.CommitAsync();

                return (applicationUser.Id, applicationUser.Email);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        // ===== Recuperação de senha =====
        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return token;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }

        // JWT Methods Implementation
        public async Task<string> GenerateJwtToken(string email, User domainUser)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return string.Empty;
            IList<string> roles = await _userManager.GetRolesAsync(user);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT:SecretKey"] ?? "your-secret-key-here");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, domainUser.Name),
                new Claim(ClaimTypes.NameIdentifier, domainUser.Id.ToString()),
                new Claim("userId", domainUser.Id.ToString()), 
                new Claim("tenantId", domainUser.TenantId.ToString()),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var subject = new ClaimsIdentity(claims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Expires = DateTime.UtcNow.AddMinutes(15), // 15 minutes
                Issuer = _configuration["JWT:Issuer"],
                Audience = _configuration["JWT:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var base64Token = Convert.ToBase64String(randomNumber);
            return base64Token;
        }

        public async Task<bool> ValidateRefreshToken(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow);

            return token != null;
        }

        public async Task<string?> GetEmailFromRefreshToken(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow);

            return token?.Email;
        }

        public async Task SaveRefreshToken(string email, string refreshToken, int? tenantId)
        {
            // Remove existing refresh tokens for this user
            var existingTokens = _context.RefreshTokens.Where(rt => rt.Email == email && rt.TenantId == tenantId);
            _context.RefreshTokens.RemoveRange(existingTokens);

            // Add new refresh token
            var newRefreshToken = new RefreshToken
            {
                Token = refreshToken,
                TenantId = tenantId,
                Email = email,
                ExpiryDate = DateTime.UtcNow.AddDays(7), // 7 days
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveRefreshToken(string refreshToken)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (token == null) return false;

            token.IsRevoked = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConfirmEmail(string userId, string token)
        {
            var applicationUser = await _userManager.FindByIdAsync(userId);
            if (applicationUser == null) return false;

            var result = await _userManager.ConfirmEmailAsync(applicationUser, token);
            return result.Succeeded;
        }
    }
}
