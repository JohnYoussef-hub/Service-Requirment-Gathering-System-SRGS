using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SRGS.Domain.Requests;
using SRGS.Domain.Requests.Lookups;
using SRGS.Domain.Users;

namespace SRGS.Infrastructure.Persistence.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToTable("REQUEST", t =>
        {
            t.HasCheckConstraint("CK_REQUEST_priority", "priority IN ('Low','Medium','High','Critical')");
            t.HasCheckConstraint("CK_REQUEST_progress", "development_progress BETWEEN 0 AND 100");
            t.HasCheckConstraint("CK_REQUEST_uat", "uat_result IN ('Passed','Failed','Pending')");
        });

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("request_id").ValueGeneratedOnAdd();

        builder.Property(r => r.CreatedDate)
            .HasColumnName("created_date")
            .HasColumnType("date")
            .HasDefaultValueSql("CAST(SYSUTCDATETIME() AS DATE)")
            .IsRequired();

        // PERSISTED computed column, exact expression from the DDL — request_code and
        // request_id here are SQL column names (the computed SQL runs inside the DB),
        // not the EF/C# property names. stored: true = PERSISTED, so it's queryable/
        // indexable and EF reads the value back after insert instead of computing it
        // client-side.
        builder.Property(r => r.RequestCode)
            .HasColumnName("request_code")
            .HasMaxLength(50)
            .HasComputedColumnSql(
                "CAST('RF-' + CONVERT(VARCHAR(8), created_date, 112) + '-' " +
                "+ RIGHT('0000' + CAST(request_id AS VARCHAR(10)), 4) AS NVARCHAR(50))",
                stored: true);

        builder.Property(r => r.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(r => r.Description).HasColumnName("description").IsRequired();

        builder.Property(r => r.RequestTypeId).HasColumnName("type_id").IsRequired();
        builder.Property(r => r.RequestedById).HasColumnName("requested_by").IsRequired();
        builder.Property(r => r.ImpactedModuleTypeId).HasColumnName("impacted_module_id").IsRequired();

        builder.Property(r => r.CurrentBehavior).HasColumnName("current_behavior");
        builder.Property(r => r.ExpectedBehavior).HasColumnName("expected_behavior");
        builder.Property(r => r.BusinessJustification).HasColumnName("business_justification").IsRequired();

        builder.Property(r => r.Priority)
            .HasColumnName("priority")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Domain.Requests.Enums.RequestPriority.Low)
            .IsRequired();

        // status/phase have no CHECK constraint in the DB yet (a gap you already flagged) —
        // mapped as plain NVARCHAR via string conversion so new enum members don't need a
        // migration, add the CHECK later once RequestStatus/RequestPhase are confirmed final.
        //
        // Default changed from the DDL's literal 'Under Analysis' -> 'UnderAnalysis' (no
        // space) to match the C# enum name exactly, since HasConversion<string>() serializes
        // RequestStatus.UnderAnalysis as "UnderAnalysis". The app always sets Status/Phase
        // explicitly in Request.Create(), so this default is really only a safety net for
        // a row inserted outside the app — but it has to match what the app itself writes,
        // or that safety net is inconsistent with real data. Fix the column default if it
        // hasn't been created yet; if you already deployed the original DDL, this migration
        // just corrects it.
        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(Domain.Requests.Enums.RequestStatus.UnderAnalysis)
            .IsRequired();

        builder.Property(r => r.Phase)
            .HasColumnName("phase")
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(Domain.Requests.Enums.RequestPhase.Logging)
            .IsRequired();

        builder.Property(r => r.AssignedDeveloperId).HasColumnName("assigned_developer");
        builder.Property(r => r.AssignedBusinessAnalystId).HasColumnName("assigned_ba");

        builder.Property(r => r.EstimatedEffort).HasColumnName("estimated_effort").HasColumnType("decimal(6,2)");
        builder.Property(r => r.ActualStart).HasColumnName("actual_start");
        builder.Property(r => r.ActualEnd).HasColumnName("actual_end");

        builder.Property(r => r.OpenTimeUtc)
            .HasColumnName("open_time")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(r => r.CloseTimeUtc).HasColumnName("close_time");

        builder.Property(r => r.DevelopmentProgress).HasColumnName("development_progress").HasColumnType("tinyint");

        builder.Property(r => r.UatResult)
            .HasColumnName("uat_result")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne<RequestType>().WithMany().HasForeignKey(r => r.RequestTypeId)
            .HasConstraintName("FK_REQUEST_TYPE").OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>().WithMany().HasForeignKey(r => r.RequestedById)
            .HasConstraintName("FK_REQUEST_REQUESTER").OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>().WithMany().HasForeignKey(r => r.AssignedDeveloperId)
            .HasConstraintName("FK_REQUEST_DEVELOPER").OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>().WithMany().HasForeignKey(r => r.AssignedBusinessAnalystId)
            .HasConstraintName("FK_REQUEST_BA").OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ModuleType>().WithMany().HasForeignKey(r => r.ImpactedModuleTypeId)
            .HasConstraintName("FK_REQUEST_MODULE").OnDelete(DeleteBehavior.Restrict);

        // FR-004 / FR-016 — "my requests" and dashboard/status queries, per the original DDL comment.
        builder.HasIndex(r => r.RequestedById).HasDatabaseName("IX_REQUEST_requested_by");
        builder.HasIndex(r => r.Status).HasDatabaseName("IX_REQUEST_status");

        // Pending schema improvement you already flagged: FK columns have no index by
        // default in SQL Server. Adding the ones the DDL didn't have yet.
        builder.HasIndex(r => r.RequestTypeId).HasDatabaseName("IX_REQUEST_type_id");
        builder.HasIndex(r => r.ImpactedModuleTypeId).HasDatabaseName("IX_REQUEST_impacted_module_id");
    }
}
