using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SRGS.Domain.Requests.Lookups;

namespace SRGS.Infrastructure.Persistence.Configurations;

public class ModuleTypeConfiguration : IEntityTypeConfiguration<ModuleType>
{
    public void Configure(EntityTypeBuilder<ModuleType> builder)
    {
        builder.ToTable("MODULE_TYPE");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("module_id").ValueGeneratedOnAdd();

        builder.Property(m => m.ModuleName).HasColumnName("module_name").HasMaxLength(50).IsRequired();

        builder.HasIndex(m => m.ModuleName).IsUnique();
    }
}
