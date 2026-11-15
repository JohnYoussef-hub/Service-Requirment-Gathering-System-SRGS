using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRGS.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestWorkflowPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "REQUEST",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Submitted",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "UnderAnalysis");

            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                table: "REQUEST",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "new_status",
                table: "CHANGE_HISTORY",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "old_status",
                table: "CHANGE_HISTORY",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [REQUEST]
                SET [status] = CASE [status]
                    WHEN 'UnderAnalysis' THEN 'Submitted'
                    WHEN 'Under Analysis' THEN 'Submitted'
                    WHEN 'PendingApproval' THEN 'ManagementApproval'
                    WHEN 'Pending Approval' THEN 'ManagementApproval'
                    WHEN 'Approved' THEN 'Assigned'
                    WHEN 'OnHold' THEN 'InDevelopment'
                    WHEN 'On Hold' THEN 'InDevelopment'
                    WHEN 'InUat' THEN 'UAT'
                    WHEN 'In UAT' THEN 'UAT'
                    WHEN 'Completed' THEN 'Closed'
                    ELSE [status]
                END
                WHERE [status] IN (
                    'UnderAnalysis',
                    'Under Analysis',
                    'PendingApproval',
                    'Pending Approval',
                    'Approved',
                    'OnHold',
                    'On Hold',
                    'InUat',
                    'In UAT',
                    'Completed');
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_REQUEST_status",
                table: "REQUEST",
                sql: "status IN ('Submitted','UnderReview','ClarificationRequired','Analysis','ManagementApproval','StakeholdersApproval','Assigned','InDevelopment','InternalTesting','UAT','Deployed','Closed','Rejected','Cancelled')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_REQUEST_status",
                table: "REQUEST");

            migrationBuilder.DropColumn(
                name: "row_version",
                table: "REQUEST");

            migrationBuilder.DropColumn(
                name: "new_status",
                table: "CHANGE_HISTORY");

            migrationBuilder.DropColumn(
                name: "old_status",
                table: "CHANGE_HISTORY");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "REQUEST",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "UnderAnalysis",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Submitted");
        }
    }
}
