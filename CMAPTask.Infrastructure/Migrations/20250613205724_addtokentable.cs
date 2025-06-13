using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBanking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addtokentable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyEndUsers_Users_UserId",
                table: "CompanyEndUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Credits_Users_UserId",
                table: "Credits");

            migrationBuilder.DropForeignKey(
                name: "FK_GoCardlessSettings_Users_UserId",
                table: "GoCardlessSettings");

            migrationBuilder.DropIndex(
                name: "IX_GoCardlessSettings_UserId",
                table: "GoCardlessSettings");

            migrationBuilder.DropIndex(
                name: "IX_Credits_UserId",
                table: "Credits");

            migrationBuilder.DropIndex(
                name: "IX_CompanyEndUsers_UserId",
                table: "CompanyEndUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_GoCardlessSettings_UserId",
                table: "GoCardlessSettings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Credits_UserId",
                table: "Credits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyEndUsers_UserId",
                table: "CompanyEndUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyEndUsers_Users_UserId",
                table: "CompanyEndUsers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Credits_Users_UserId",
                table: "Credits",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoCardlessSettings_Users_UserId",
                table: "GoCardlessSettings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
