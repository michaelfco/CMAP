using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBanking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removerequiredstartandenddate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CompanyEndUsers_EndUserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_EndUserId",
                table: "Transactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Transactions_EndUserId",
                table: "Transactions",
                column: "EndUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CompanyEndUsers_EndUserId",
                table: "Transactions",
                column: "EndUserId",
                principalTable: "CompanyEndUsers",
                principalColumn: "EndUserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
