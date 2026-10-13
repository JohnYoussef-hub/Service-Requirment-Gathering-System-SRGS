using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRGS.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenFamilies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "family_id",
                table: "REFRESH_TOKEN",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<bool>(
                name: "is_revoked",
                table: "REFRESH_TOKEN",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_used",
                table: "REFRESH_TOKEN",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE [REFRESH_TOKEN] SET [token] = CONVERT(varchar(64), HASHBYTES('SHA2_256', CONVERT(varchar(max), [token])), 2)");

            migrationBuilder.CreateIndex(
                name: "IX_REFRESH_TOKEN_family_id",
                table: "REFRESH_TOKEN",
                column: "family_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_REFRESH_TOKEN_family_id",
                table: "REFRESH_TOKEN");

            migrationBuilder.DropColumn(
                name: "family_id",
                table: "REFRESH_TOKEN");

            migrationBuilder.DropColumn(
                name: "is_revoked",
                table: "REFRESH_TOKEN");

            migrationBuilder.DropColumn(
                name: "is_used",
                table: "REFRESH_TOKEN");
        }
    }
}
