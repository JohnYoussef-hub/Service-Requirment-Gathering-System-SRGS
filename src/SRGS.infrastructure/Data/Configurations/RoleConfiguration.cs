using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SRGS.Domain.Roles;

namespace SRGS.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("ROLE");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("role_id").ValueGeneratedOnAdd();

        builder.Property(r => r.RoleName).HasColumnName("role_name").HasMaxLength(50).IsRequired();

        builder.HasIndex(r => r.RoleName).IsUnique();
    }
}
