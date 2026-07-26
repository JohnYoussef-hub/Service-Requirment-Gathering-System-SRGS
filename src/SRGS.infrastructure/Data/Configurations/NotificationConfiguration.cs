using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SRGS.Domain.Requests;
using SRGS.Domain.Requests.Notifications;
using SRGS.Domain.Users;

namespace SRGS.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("NOTIFICATION", t =>
            t.HasCheckConstraint("CK_NOTIFICATION_status", "status IN ('Pending','Sent','Failed')"));

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("notification_id").ValueGeneratedOnAdd();

        builder.Property(n => n.RequestId).HasColumnName("request_id").IsRequired();
        builder.Property(n => n.EventType).HasColumnName("event_type").HasMaxLength(50).IsRequired();

        builder.Property(n => n.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Domain.Requests.Notifications.Enums.NotificationStatus.Pending)
            .IsRequired();

        builder.Property(n => n.RecipientUserId).HasColumnName("recipient").IsRequired();
        builder.Property(n => n.SentAtUtc).HasColumnName("sent_at");

        builder.HasOne<Request>().WithMany().HasForeignKey(n => n.RequestId)
            .HasConstraintName("FK_NOTIFICATION_REQUEST").OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>().WithMany().HasForeignKey(n => n.RecipientUserId)
            .HasConstraintName("FK_RECIPIENT").OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => n.RequestId).HasDatabaseName("IX_NOTIFICATION_request_id");
    }
}
