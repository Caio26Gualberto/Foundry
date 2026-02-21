using Boilerplate.Domain.Constants;
using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Enums;
using Boilerplate.Domain.Interfaces.Authenticate;
using Boilerplate.Infra.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
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

        public async Task<(bool, bool)> Authenticate(string email, string password)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return (false, false);

            var passwordValid = await _userManager.CheckPasswordAsync(user, password);

            return (passwordValid, user.IsNeededChangePassword);
        }

        public async Task<(int, string)> RegisterTenantAdmin(string email, string password, string name, int tenantId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    Email = email,
                    Name = name,
                    TenantId = tenantId
                };

                _context.DomainUsers.Add(user);
                await _context.SaveChangesAsync();

                var applicationUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    DomainUserId = user.Id,
                    TenantId = tenantId,
                    IsNeededChangePassword = true
                };

                var result = await _userManager.CreateAsync(applicationUser, password);
                await _userManager.AddToRoleAsync(applicationUser, Roles.TenantAdmin);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return (0, string.Empty);
                }
                await transaction.CommitAsync();

                return (applicationUser.Id, applicationUser.Email);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(int, string)> Register(string email, string password, string name, int tenantId, string token)
        {
            var invite = await _context.TenantInvitations.FirstOrDefaultAsync(x => x.Token == token);
            if (invite == null || invite.TenantId != tenantId)
                return (0, string.Empty);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    Email = email,
                    Name = name,
                    TenantId = invite.TenantId
                };

                _context.DomainUsers.Add(user);
                await _context.SaveChangesAsync();

                var applicationUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    DomainUserId = user.Id,
                    TenantId = invite.TenantId
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
                new Claim("tenantName", domainUser?.Tenant?.Name ?? string.Empty),
                new Claim("tenantId", domainUser?.TenantId?.ToString() ?? string.Empty),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var subject = new ClaimsIdentity(claims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Expires = DateTime.UtcNow.AddHours(3),
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

        public async Task<bool> IsExpiredRefreshToken(string refreshToken)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (token == null) return false;

            return token.ExpiryDate < DateTime.Now;
        }

        public ClaimsPrincipal GetClaimsByToken(string impersonatedToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(impersonatedToken);
            var identity = new ClaimsIdentity(jwt.Claims);
            return new ClaimsPrincipal(identity);
        }

        public async Task<string> GenerateJwtImpersonatorToken(User adminUser, User targetUser)
        {
            if (adminUser == null) throw new ArgumentNullException(nameof(adminUser));
            if (targetUser == null) throw new ArgumentNullException(nameof(targetUser));
            var targetApplicationUSer = await _userManager.FindByEmailAsync(targetUser.Email);
            if (targetApplicationUSer == null) throw new ArgumentException(nameof(targetUser));
            IList<string> roles = await _userManager.GetRolesAsync(targetApplicationUSer);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, targetUser.Name),
                new Claim(ClaimTypes.NameIdentifier, targetUser.Id.ToString()),
                new Claim("userId", targetUser.Id.ToString()),
                new Claim("tenantId", targetUser.TenantId?.ToString() ?? string.Empty),
                new Claim("tenantName", targetUser.Tenant!.Name),
                new Claim(ClaimTypes.Email, targetUser.Email ?? string.Empty),
                new Claim("impersonatedBy", adminUser.Id.ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:SecretKey"] ?? "your-secret-key-here"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> ValidateInviteToken(string email, string token)
        {
            var invite = await _context.TenantInvitations.FirstOrDefaultAsync(x => x.Token == token);
            if (invite == null)
                return false;

            if (invite.ExpiresAt < DateTime.Now)
                return false;

            if (invite.Status == EInviteStatus.Expired || invite.Status == EInviteStatus.Cancelled)
                return false;

            return true;
        }
        public async Task<bool> ChangePassword(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            await _userManager.RemovePasswordAsync(user);

            var result = await _userManager.AddPasswordAsync(user, password);
            if (!result.Succeeded)
                return false;

            user.IsNeededChangePassword = false;
            await _userManager.UpdateAsync(user);

            return true;
        }
    }
}
