using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SRGS.Domain.Requests;
using SRGS.Domain.Requests.History;
using SRGS.Domain.Requests.History.Enums;
using SRGS.Domain.Users;

namespace SRGS.Infrastructure.Persistence.Configurations;

public class ChangeHistoryConfiguration : IEntityTypeConfiguration<ChangeHistory>
{
    public void Configure(EntityTypeBuilder<ChangeHistory> builder)
    {
        builder.ToTable("CHANGE_HISTORY", t =>
        {
            t.HasCheckConstraint("CK_CHANGE_operation", "change_operation IN ('Add','Update','Delete')");
            t.HasCheckConstraint("CK_CHANGE_type",
                "change_type IN ('Rule Set','Model','Form Designer','Workflow','Script','Catalog Item','Integration')");
        });

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("change_id").ValueGeneratedOnAdd();

        builder.Property(c => c.RequestId).HasColumnName("request_id").IsRequired();

        builder.Property(c => c.ChangeOperation)
            .HasColumnName("change_operation")
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        // CHECK constraint uses spaced values ('Rule Set', 'Form Designer', 'Catalog Item')
        // but the C# enum members can't contain spaces (RuleSet, FormDesigner, CatalogItem).
        // A plain HasConversion<string>() would write "RuleSet" and violate the CHECK
        // constraint on every insert — so this needs an explicit two-way map instead of the
        // default enum-to-name conversion.
        builder.Property(c => c.ChangeType)
            .HasColumnName("change_type")
            .HasConversion(
                v => v.HasValue ? ChangeTypeToDb[v.Value] : null,
                v => v == null ? null : (ChangeType?)DbToChangeType[v])
            .HasMaxLength(50);

        builder.Property(c => c.ChangedById).HasColumnName("changed_by").IsRequired();
        builder.Property(c => c.Comment).HasColumnName("comment");
        builder.Property(c => c.ChangedAtUtc).HasColumnName("changed_at").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne<Request>().WithMany().HasForeignKey(c => c.RequestId)
            .HasConstraintName("FK_CHANGE_REQUEST").OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>().WithMany().HasForeignKey(c => c.ChangedById)
            .HasConstraintName("FK_CHANGE_USER").OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.RequestId).HasDatabaseName("IX_CHANGE_HISTORY_request_id");
    }

    private static readonly Dictionary<ChangeType, string> ChangeTypeToDb = new()
    {
        [ChangeType.RuleSet] = "Rule Set",
        [ChangeType.Model] = "Model",
        [ChangeType.FormDesigner] = "Form Designer",
        [ChangeType.Workflow] = "Workflow",
        [ChangeType.Script] = "Script",
        [ChangeType.CatalogItem] = "Catalog Item",
        [ChangeType.Integration] = "Integration"
    };

    private static readonly Dictionary<string, ChangeType> DbToChangeType =
        ChangeTypeToDb.ToDictionary(kv => kv.Value, kv => kv.Key);
}
