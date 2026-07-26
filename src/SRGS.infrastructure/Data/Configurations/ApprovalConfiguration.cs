using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SRGS.Domain.Requests;
using SRGS.Domain.Requests.Approvals;
using SRGS.Domain.Requests.Approvals.Enums;
using SRGS.Domain.Users;

namespace SRGS.Infrastructure.Persistence.Configurations;

public class ApprovalConfiguration : IEntityTypeConfiguration<Approval>
{
    public void Configure(EntityTypeBuilder<Approval> builder)
    {
        builder.ToTable("APPROVAL", t =>
        {
            t.HasCheckConstraint("CK_APPROVAL_TYPE_decision", "approval_type IN ('Stakeholder','Managerial')");
            t.HasCheckConstraint("CK_APPROVAL_decision", "decision IN ('Approved','Rejected','Pending')");
        });

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("approval_id").ValueGeneratedOnAdd();

        builder.Property(a => a.RequestId).HasColumnName("request_id").IsRequired();
        builder.Property(a => a.ApproverId).HasColumnName("approver").IsRequired();

        // DDL has approval_type/decision as NULL-able (decision defaults 'Pending').
        // Approval.Create() never leaves either unset, so tightened both to NOT NULL here —
        // an approval that exists without a type, or without at least a Pending decision,
        // isn't a state the domain can produce. If you already deployed the original
        // nullable DDL, this migration corrects it; flagging so it's not a silent change.
        builder.Property(a => a.ApprovalType)
            .HasColumnName("approval_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Decision)
            .HasColumnName("decision")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ApprovalDecisionStatus.Pending)
            .IsRequired();

        builder.Property(a => a.DecidedAtUtc).HasColumnName("decided_at");

        builder.HasOne<Request>().WithMany().HasForeignKey(a => a.RequestId)
            .HasConstraintName("FK_APPROVAL_REQUEST").OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>().WithMany().HasForeignKey(a => a.ApproverId)
            .HasConstraintName("FK_APPROVAL_USER").OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.RequestId).HasDatabaseName("IX_APPROVAL_request_id");
    }
}
