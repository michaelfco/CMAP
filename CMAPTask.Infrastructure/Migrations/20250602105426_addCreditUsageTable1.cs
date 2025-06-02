using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBanking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addCreditUsageTable1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_BankAccounts_BankAccountId1",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_BankAccountId1",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "BankAccountId1",
                table: "BankTransactions");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Credits",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Credits");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountId1",
                table: "BankTransactions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_BankAccountId1",
                table: "BankTransactions",
                column: "BankAccountId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransactions_BankAccounts_BankAccountId1",
                table: "BankTransactions",
                column: "BankAccountId1",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
