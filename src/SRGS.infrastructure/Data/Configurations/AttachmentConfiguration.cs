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
        builder.ToTable("ATTACHMENT", t =>
            t.HasCheckConstraint("CHK_ATTACHMENT_FILE_SIZE", $"file_size > 0 AND file_size <= {Attachment.MaxFileSizeBytes}"));

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("attachment_id").ValueGeneratedOnAdd();

        builder.Property(a => a.FileName).HasColumnName("file_name").HasMaxLength(500).IsRequired();
        builder.Property(a => a.FileExtension).HasColumnName("file_extension").HasMaxLength(50);
        builder.Property(a => a.MimeType).HasColumnName("mime_type").HasMaxLength(100).IsRequired();
        builder.Property(a => a.FileSize).HasColumnName("file_size").IsRequired();

        // VARBINARY(MAX) <-> byte[] is EF Core's default mapping for byte[], so no
        // .HasColumnType() call is strictly needed — added anyway to make the intent
        // explicit rather than relying on convention.
        builder.Property(a => a.FileData).HasColumnName("file_data").HasColumnType("varbinary(max)").IsRequired();

        builder.Property(a => a.RequestId).HasColumnName("request_id").IsRequired();
        builder.Property(a => a.UploadedByUserId).HasColumnName("uploaded_by").IsRequired();
        builder.Property(a => a.UploadedAtUtc).HasColumnName("uploaded_at").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne<Request>().WithMany().HasForeignKey(a => a.RequestId)
            .HasConstraintName("FK_ATTACHMENT_REQUEST").OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>().WithMany().HasForeignKey(a => a.UploadedByUserId)
            .HasConstraintName("FK_ATTACHMENT_USER").OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.RequestId).HasDatabaseName("IX_ATTACHMENT_request_id");
    }
}
