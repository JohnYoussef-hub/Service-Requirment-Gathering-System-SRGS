using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRGS.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MODULE_TYPE",
                columns: table => new
                {
                    module_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    module_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MODULE_TYPE", x => x.module_id);
                });

            migrationBuilder.CreateTable(
                name: "REQUEST_TYPE",
                columns: table => new
                {
                    type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REQUEST_TYPE", x => x.type_id);
                });

            migrationBuilder.CreateTable(
                name: "ROLE",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLE", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "USER",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    middle_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    family_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    sub_department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    employee_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "REFRESH_TOKEN",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    expires_on_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    last_modified_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    last_modified_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REFRESH_TOKEN", x => x.id);
                    table.ForeignKey(
                        name: "FK_REFRESH_TOKEN_USER",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "REQUEST",
                columns: table => new
                {
                    request_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    request_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, computedColumnSql: "CAST('RF-' + CONVERT(VARCHAR(8), created_date, 112) + '-' + RIGHT('0000' + CAST(request_id AS VARCHAR(10)), 4) AS NVARCHAR(50))", stored: true),
                    created_date = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CAST(SYSUTCDATETIME() AS DATE)"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    type_id = table.Column<int>(type: "int", nullable: false),
                    requested_by = table.Column<int>(type: "int", nullable: false),
                    impacted_module_id = table.Column<int>(type: "int", nullable: false),
                    current_behavior = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    expected_behavior = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    business_justification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Low"),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "UnderAnalysis"),
                    phase = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Logging"),
                    assigned_developer = table.Column<int>(type: "int", nullable: true),
                    assigned_ba = table.Column<int>(type: "int", nullable: true),
                    estimated_effort = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    actual_start = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    actual_end = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    open_time = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    close_time = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    development_progress = table.Column<byte>(type: "tinyint", nullable: true),
                    uat_result = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REQUEST", x => x.request_id);
                    table.CheckConstraint("CK_REQUEST_priority", "priority IN ('Low','Medium','High','Critical')");
                    table.CheckConstraint("CK_REQUEST_progress", "development_progress BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_REQUEST_uat", "uat_result IN ('Passed','Failed','Pending')");
                    table.ForeignKey(
                        name: "FK_REQUEST_BA",
                        column: x => x.assigned_ba,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REQUEST_DEVELOPER",
                        column: x => x.assigned_developer,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REQUEST_MODULE",
                        column: x => x.impacted_module_id,
                        principalTable: "MODULE_TYPE",
                        principalColumn: "module_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REQUEST_REQUESTER",
                        column: x => x.requested_by,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REQUEST_TYPE",
                        column: x => x.type_id,
                        principalTable: "REQUEST_TYPE",
                        principalColumn: "type_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "USER_ROLE",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_ROLE", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_USER_ROLE_ROLE_role_id",
                        column: x => x.role_id,
                        principalTable: "ROLE",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_USER_ROLE_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "APPROVAL",
                columns: table => new
                {
                    approval_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    request_id = table.Column<int>(type: "int", nullable: false),
                    approver = table.Column<int>(type: "int", nullable: false),
                    approval_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    decision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    decided_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APPROVAL", x => x.approval_id);
                    table.CheckConstraint("CK_APPROVAL_decision", "decision IN ('Approved','Rejected','Pending')");
                    table.CheckConstraint("CK_APPROVAL_TYPE_decision", "approval_type IN ('Stakeholder','Managerial')");
                    table.ForeignKey(
                        name: "FK_APPROVAL_REQUEST",
                        column: x => x.request_id,
                        principalTable: "REQUEST",
                        principalColumn: "request_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_APPROVAL_USER",
                        column: x => x.approver,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ATTACHMENT",
                columns: table => new
                {
                    attachment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    request_id = table.Column<int>(type: "int", nullable: false),
                    uploaded_by = table.Column<int>(type: "int", nullable: false),
                    file_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    file_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    uploaded_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ATTACHMENT", x => x.attachment_id);
                    table.ForeignKey(
                        name: "FK_ATTACHMENT_REQUEST",
                        column: x => x.request_id,
                        principalTable: "REQUEST",
                        principalColumn: "request_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ATTACHMENT_USER",
                        column: x => x.uploaded_by,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CHANGE_HISTORY",
                columns: table => new
                {
                    change_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    request_id = table.Column<int>(type: "int", nullable: false),
                    change_operation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    change_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    changed_by = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    changed_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHANGE_HISTORY", x => x.change_id);
                    table.CheckConstraint("CK_CHANGE_operation", "change_operation IN ('Add','Update','Delete')");
                    table.CheckConstraint("CK_CHANGE_type", "change_type IN ('Rule Set','Model','Form Designer','Workflow','Script','Catalog Item','Integration')");
                    table.ForeignKey(
                        name: "FK_CHANGE_REQUEST",
                        column: x => x.request_id,
                        principalTable: "REQUEST",
                        principalColumn: "request_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CHANGE_USER",
                        column: x => x.changed_by,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DESCRIPTION",
                columns: table => new
                {
                    description_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    request_id = table.Column<int>(type: "int", nullable: false),
                    author = table.Column<int>(type: "int", nullable: false),
                    description_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DESCRIPTION", x => x.description_id);
                    table.ForeignKey(
                        name: "FK_DESCRIPTION_REQUEST",
                        column: x => x.request_id,
                        principalTable: "REQUEST",
                        principalColumn: "request_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DESCRIPTION_USER",
                        column: x => x.author,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICATION",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    request_id = table.Column<int>(type: "int", nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    recipient = table.Column<int>(type: "int", nullable: false),
                    sent_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICATION", x => x.notification_id);
                    table.CheckConstraint("CK_NOTIFICATION_status", "status IN ('Pending','Sent','Failed')");
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_REQUEST",
                        column: x => x.request_id,
                        principalTable: "REQUEST",
                        principalColumn: "request_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RECIPIENT",
                        column: x => x.recipient,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APPROVAL_approver",
                table: "APPROVAL",
                column: "approver");

            migrationBuilder.CreateIndex(
                name: "IX_APPROVAL_request_id",
                table: "APPROVAL",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_ATTACHMENT_request_id",
                table: "ATTACHMENT",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_ATTACHMENT_uploaded_by",
                table: "ATTACHMENT",
                column: "uploaded_by");

            migrationBuilder.CreateIndex(
                name: "IX_CHANGE_HISTORY_changed_by",
                table: "CHANGE_HISTORY",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_CHANGE_HISTORY_request_id",
                table: "CHANGE_HISTORY",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_DESCRIPTION_author",
                table: "DESCRIPTION",
                column: "author");

            migrationBuilder.CreateIndex(
                name: "IX_DESCRIPTION_request_id",
                table: "DESCRIPTION",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_MODULE_TYPE_module_name",
                table: "MODULE_TYPE",
                column: "module_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_recipient",
                table: "NOTIFICATION",
                column: "recipient");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_request_id",
                table: "NOTIFICATION",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_REFRESH_TOKEN_token",
                table: "REFRESH_TOKEN",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_REFRESH_TOKEN_user_id",
                table: "REFRESH_TOKEN",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_REQUEST_assigned_ba",
                table: "REQUEST",
                column: "assigned_ba");

            migrationBuilder.CreateIndex(
                name: "IX_REQUEST_assigned_developer",
                table: "REQUEST",
                column: "assigned_developer");

            migrationBuilder.CreateIndex(
                name: "IX_REQUEST_impacted_module_id",
                table: "REQUEST",
                column: "impacted_module_id");

            migrationBuilder.CreateIndex(
                name: "IX_REQUEST_requested_by",
                table: "REQUEST",
                column: "requested_by");

            migrationBuilder.CreateIndex(
                name: "IX_REQUEST_status",
                table: "REQUEST",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_REQUEST_type_id",
                table: "REQUEST",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_REQUEST_TYPE_type_name",
                table: "REQUEST_TYPE",
                column: "type_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_role_name",
                table: "ROLE",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_email",
                table: "USER",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_employee_number",
                table: "USER",
                column: "employee_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_username",
                table: "USER",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_ROLE_role_id",
                table: "USER_ROLE",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "APPROVAL");

            migrationBuilder.DropTable(
                name: "ATTACHMENT");

            migrationBuilder.DropTable(
                name: "CHANGE_HISTORY");

            migrationBuilder.DropTable(
                name: "DESCRIPTION");

            migrationBuilder.DropTable(
                name: "NOTIFICATION");

            migrationBuilder.DropTable(
                name: "REFRESH_TOKEN");

            migrationBuilder.DropTable(
                name: "USER_ROLE");

            migrationBuilder.DropTable(
                name: "REQUEST");

            migrationBuilder.DropTable(
                name: "ROLE");

            migrationBuilder.DropTable(
                name: "USER");

            migrationBuilder.DropTable(
                name: "MODULE_TYPE");

            migrationBuilder.DropTable(
                name: "REQUEST_TYPE");
        }
    }
}
