using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMAPTask.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Timesheet",
                table: "Timesheet");

            migrationBuilder.RenameTable(
                name: "Timesheet",
                newName: "Timesheet");

            migrationBuilder.RenameIndex(
                name: "IX_Timesheet_UserName_Date",
                table: "Timesheet",
                newName: "IX_Timesheet_UserName_Date");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Timesheet",
                table: "Timesheet",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Timesheet",
                table: "Timesheet");

            migrationBuilder.RenameTable(
                name: "Timesheet",
                newName: "Timesheet");

            migrationBuilder.RenameIndex(
                name: "IX_Timesheet_UserName_Date",
                table: "Timesheet",
                newName: "IX_Timesheet_UserName_Date");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Timesheet",
                table: "Timesheet",
                column: "Id");
        }
    }
}
