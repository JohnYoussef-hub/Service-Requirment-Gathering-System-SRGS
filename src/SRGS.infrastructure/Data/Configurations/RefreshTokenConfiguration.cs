using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SRGS.Domain.Identity;
using SRGS.Domain.Users;

namespace SRGS.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("REFRESH_TOKEN");

        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id).HasColumnName("id").HasDefaultValueSql("NEWID()");

        builder.Property(rt => rt.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
        builder.Property(rt => rt.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(rt => rt.ExpiresOnUtc).HasColumnName("expires_on_utc").IsRequired();

        builder.Property(rt => rt.CreatedAtUtc).HasColumnName("created_at_utc").HasDefaultValueSql("SYSDATETIMEOFFSET()").IsRequired();
        builder.Property(rt => rt.CreatedBy).HasColumnName("created_by").HasMaxLength(50);
        builder.Property(rt => rt.LastModifiedUtc).HasColumnName("last_modified_utc");
        builder.Property(rt => rt.LastModifiedBy).HasColumnName("last_modified_by").HasMaxLength(50);

        builder.HasOne<User>().WithMany().HasForeignKey(rt => rt.UserId)
            .HasConstraintName("FK_REFRESH_TOKEN_USER").OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rt => rt.Token).IsUnique().HasDatabaseName("IX_REFRESH_TOKEN_token");
        builder.HasIndex(rt => rt.UserId).HasDatabaseName("IX_REFRESH_TOKEN_user_id");
    }
}
