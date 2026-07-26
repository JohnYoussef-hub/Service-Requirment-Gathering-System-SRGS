using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SRGS.Domain.Requests;
using SRGS.Domain.Requests.Notes;
using SRGS.Domain.Users;

namespace SRGS.Infrastructure.Persistence.Configurations;

public class RequestDescriptionConfiguration : IEntityTypeConfiguration<Description>
{
    public void Configure(EntityTypeBuilder<Description> builder)
    {
        // C# type is RequestNote (see SRGS.Domain README on this rename); table stays
        // DESCRIPTION so no schema change is needed for a naming decision made in code.
        builder.ToTable("DESCRIPTION");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("description_id").ValueGeneratedOnAdd();

        builder.Property(n => n.RequestId).HasColumnName("request_id").IsRequired();
        builder.Property(n => n.AuthorId).HasColumnName("author").IsRequired();
        builder.Property(n => n.DescriptionText).HasColumnName("description_text").IsRequired();
        builder.Property(n => n.CreatedAtUtc).HasColumnName("created_at").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne<Request>().WithMany().HasForeignKey(n => n.RequestId)
            .HasConstraintName("FK_DESCRIPTION_REQUEST").OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>().WithMany().HasForeignKey(n => n.AuthorId)
            .HasConstraintName("FK_DESCRIPTION_USER").OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => n.RequestId).HasDatabaseName("IX_DESCRIPTION_request_id");
    }
}
