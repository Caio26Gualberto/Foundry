using Boilerplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Boilerplate.Infra.Data.Persistence.Configuration
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(t => t.IsActive)
                   .IsRequired();

            builder.OwnsOne(t => t.Address, a =>
            {
                a.Property(p => p.Street).HasColumnName("Street").HasMaxLength(200);
                a.Property(p => p.Number).HasColumnName("Number").HasMaxLength(50);
                a.Property(p => p.City).HasColumnName("City").HasMaxLength(100);
                a.Property(p => p.State).HasColumnName("State").HasMaxLength(100);
                a.Property(p => p.Country).HasColumnName("Country").HasMaxLength(100);
                a.Property(p => p.ZipCode).HasColumnName("ZipCode").HasMaxLength(20);
            });

            builder.Navigation(t => t.Address).IsRequired();
        }
    }
}
