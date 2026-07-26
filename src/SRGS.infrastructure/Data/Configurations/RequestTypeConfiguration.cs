using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SRGS.Domain.Requests.Lookups;

namespace SRGS.Infrastructure.Persistence.Configurations;

public class RequestTypeConfiguration : IEntityTypeConfiguration<RequestType>
{
    public void Configure(EntityTypeBuilder<RequestType> builder)
    {
        builder.ToTable("REQUEST_TYPE");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("type_id").ValueGeneratedOnAdd();

        builder.Property(t => t.TypeName).HasColumnName("type_name").HasMaxLength(50).IsRequired();

        builder.HasIndex(t => t.TypeName).IsUnique();
    }
}
