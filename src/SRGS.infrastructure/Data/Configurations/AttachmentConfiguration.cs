using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SRGS.Domain.Requests;
using SRGS.Domain.Requests.Attachments;
using SRGS.Domain.Users;

namespace SRGS.Infrastructure.Persistence.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("ATTACHMENT");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("attachment_id").ValueGeneratedOnAdd();

        builder.Property(a => a.RequestId).HasColumnName("request_id").IsRequired();
        builder.Property(a => a.UploadedById).HasColumnName("uploaded_by").IsRequired();
        builder.Property(a => a.FileType).HasColumnName("file_type").HasMaxLength(50);
        builder.Property(a => a.FilePath).HasColumnName("file_path").HasMaxLength(500).IsRequired();
        builder.Property(a => a.UploadedAtUtc).HasColumnName("uploaded_at").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne<Request>().WithMany().HasForeignKey(a => a.RequestId)
            .HasConstraintName("FK_ATTACHMENT_REQUEST").OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>().WithMany().HasForeignKey(a => a.UploadedById)
            .HasConstraintName("FK_ATTACHMENT_USER").OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.RequestId).HasDatabaseName("IX_ATTACHMENT_request_id");
    }
}
