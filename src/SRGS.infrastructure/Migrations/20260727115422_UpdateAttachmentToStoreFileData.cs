using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRGS.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttachmentToStoreFileData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "file_type",
                table: "ATTACHMENT",
                newName: "file_extension");

            migrationBuilder.RenameColumn(
                name: "file_path",
                table: "ATTACHMENT",
                newName: "file_name");

            migrationBuilder.AddColumn<byte[]>(
                name: "file_data",
                table: "ATTACHMENT",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<long>(
                name: "file_size",
                table: "ATTACHMENT",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "mime_type",
                table: "ATTACHMENT",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddCheckConstraint(
                name: "CHK_ATTACHMENT_FILE_SIZE",
                table: "ATTACHMENT",
                sql: "file_size > 0 AND file_size <= 5242880");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CHK_ATTACHMENT_FILE_SIZE",
                table: "ATTACHMENT");

            migrationBuilder.DropColumn(
                name: "file_data",
                table: "ATTACHMENT");

            migrationBuilder.DropColumn(
                name: "file_size",
                table: "ATTACHMENT");

            migrationBuilder.DropColumn(
                name: "mime_type",
                table: "ATTACHMENT");

            migrationBuilder.RenameColumn(
                name: "file_name",
                table: "ATTACHMENT",
                newName: "file_path");

            migrationBuilder.RenameColumn(
                name: "file_extension",
                table: "ATTACHMENT",
                newName: "file_type");
        }
    }
}
