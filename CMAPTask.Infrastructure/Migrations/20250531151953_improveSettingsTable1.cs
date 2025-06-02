using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBanking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class improveSettingsTable1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApiSecret",
                table: "GoCardlessSettings",
                newName: "SecretID");

            migrationBuilder.RenameColumn(
                name: "ApiKey",
                table: "GoCardlessSettings",
                newName: "BaseUrl");

            migrationBuilder.AddColumn<string>(
                name: "SecretKey",
                table: "GoCardlessSettings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecretKey",
                table: "GoCardlessSettings");

            migrationBuilder.RenameColumn(
                name: "SecretID",
                table: "GoCardlessSettings",
                newName: "ApiSecret");

            migrationBuilder.RenameColumn(
                name: "BaseUrl",
                table: "GoCardlessSettings",
                newName: "ApiKey");
        }
    }
}
