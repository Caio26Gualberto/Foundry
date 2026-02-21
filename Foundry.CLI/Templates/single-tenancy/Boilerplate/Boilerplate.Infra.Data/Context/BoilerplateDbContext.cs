using Boilerplate.Application.Interfaces.ICurrentUserContext;
using Boilerplate.Domain.Entities;
using Boilerplate.Infra.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Boilerplate.Infra.Data.Context
{
    public class BoilerplateDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        private readonly ICurrentUserContext _currentUserContext;

        public BoilerplateDbContext(DbContextOptions<BoilerplateDbContext> options, ICurrentUserContext currentUserContext) : base(options) 
        { 
            _currentUserContext = currentUserContext;
        }
        public DbSet<User> DomainUsers => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<SystemNotification> SystemNotifications => Set<SystemNotification>();
        public DbSet<SystemNotificationUser> SystemNotificationUser => Set<SystemNotificationUser>();
        public DbSet<Entity1> Entity1s => Set<Entity1>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().ToTable("UsersIdentity");
            builder.Entity<IdentityRole<int>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

            builder.Entity<SystemNotificationUser>()
                .HasKey(x => new { x.UserId, x.NotificationId });

            builder.Entity<User>(b =>
            {
                b.ToTable("DomainUsers");
                b.HasKey(u => u.Id);
            });

            builder.Entity<ApplicationUser>(b =>
            {
                b.HasOne(a => a.User)
                    .WithOne()
                    .HasForeignKey<ApplicationUser>(a => a.DomainUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(a => a.UserName).IsUnique();
            });

        }

        public override int SaveChanges()
        {
            ApplyAuditing();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            ApplyAuditing();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditing()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<EntityBase>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = _currentUserContext.UserId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.UpdatedBy = _currentUserContext.UserId;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = now;
                        entry.Entity.DeletedBy = _currentUserContext.UserId;
                        break;
                }
            }
        }
    }
}
