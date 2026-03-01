using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeHrCertificationPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeIsActiveFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Employees");
        }
    }
}
