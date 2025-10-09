using Boilerplate.Domain.Entities;
using Boilerplate.Infra.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Boilerplate.Infra.Data.Context
{
    public class BoilerplateDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public BoilerplateDbContext(DbContextOptions<BoilerplateDbContext> options) : base(options) { }
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<TenantInvitation> TenantInvitations => Set<TenantInvitation>();
        public new DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();


        public int? CurrentTenantId { get; private set; }
        public void SetTenant(int tenantId) => CurrentTenantId = tenantId;

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

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
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
            return base.SaveChanges();
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
    }
}
