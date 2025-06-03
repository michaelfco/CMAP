using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBanking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addCreditUsageTable6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditId",
                table: "CreditUsages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreditId",
                table: "CreditUsages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
