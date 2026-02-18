using Boilerplate.Application.Interfaces.ICurrentUserContext;
using Boilerplate.Domain.Entities;
using Boilerplate.Infra.Data.Identity;
using Boilerplate.Infra.Data.Persistence.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Boilerplate.Infra.Data.Context
{
    public class BoilerplateDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        private readonly ICurrentUserContext _currentUserContext;

        public BoilerplateDbContext(DbContextOptions<BoilerplateDbContext> options, ICurrentUserContext currentUserContext) : base(options) 
        { 
            _currentUserContext = currentUserContext;
            CurrentTenantId = _currentUserContext.TenantId;
        }
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<TenantInvitation> TenantInvitations => Set<TenantInvitation>();
        public DbSet<User> DomainUsers => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<SystemNotification> SystemNotifications => Set<SystemNotification>();
        public DbSet<Entity1> Entity1s => Set<Entity1>();


        public int? CurrentTenantId { get; private set; }
        public void SetTenant(int tenantId) => CurrentTenantId = tenantId;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new TenantConfiguration());
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().ToTable("UsersIdentity");
            builder.Entity<IdentityRole<int>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (entityType.IsOwned())
                    continue;

                var tenantProperty = entityType.FindProperty("TenantId");
                if (tenantProperty != null && (tenantProperty.ClrType == typeof(int) || tenantProperty.ClrType == typeof(int?)))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    Expression property = Expression.Property(parameter, "TenantId");

                    if (tenantProperty.ClrType == typeof(int))
                    {
                        property = Expression.Convert(property, typeof(int?));
                    }

                    var currentTenant = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));

                    var body = Expression.OrElse(
                        Expression.Equal(currentTenant, Expression.Constant(null, typeof(int?))),
                        Expression.Equal(property, currentTenant)
                    );

                    var lambda = Expression.Lambda(body, parameter);
                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            builder.Entity<Tenant>()
                            .HasMany(t => t.Users)
                            .WithOne(u => u.Tenant)
                            .HasForeignKey(u => u.TenantId)
                            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Entity1>()
                .HasOne(e => e.Tenant)
                .WithOne()
                .HasForeignKey<Entity1>(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<User>(b =>
            {
                b.ToTable("DomainUsers");
                b.HasKey(u => u.Id);

                b.HasOne(u => u.Tenant)
                    .WithMany(t => t.Users)
                    .HasForeignKey(u => u.TenantId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });

            builder.Entity<ApplicationUser>(b =>
            {
                b.HasOne(a => a.User)
                    .WithOne()
                    .HasForeignKey<ApplicationUser>(a => a.DomainUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(a => new { a.UserName, a.TenantId }).IsUnique();

                // Tenant reference (1:N)
                b.HasOne(a => a.Tenant)
                    .WithMany()
                    .HasForeignKey(a => a.TenantId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });

        }

        public override int SaveChanges()
        {
            ApplyTenantId();
            ApplyAuditing();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            ApplyTenantId();
            ApplyAuditing();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyTenantId()
        {
            if (CurrentTenantId is null)
                return;

            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added);

            foreach (var entry in entries)
            {
                var tenantProp = entry.Entity.GetType().GetProperty("TenantId");
                if (tenantProp != null && tenantProp.GetValue(entry.Entity) == null)
                    tenantProp.SetValue(entry.Entity, CurrentTenantId);
            }
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
