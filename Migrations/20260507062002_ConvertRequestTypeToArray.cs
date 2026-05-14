using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeHrCertificationPortal.Migrations
{
    /// <inheritdoc />
    public partial class ConvertRequestTypeToArray : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""CertificationRequests"" ALTER COLUMN ""RequestType"" TYPE integer[] USING ARRAY[""RequestType""]::integer[];");
            migrationBuilder.RenameColumn(name: "RequestType", table: "CertificationRequests", newName: "RequestTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "RequestTypes", table: "CertificationRequests", newName: "RequestType");
            migrationBuilder.Sql(@"ALTER TABLE ""CertificationRequests"" ALTER COLUMN ""RequestType"" TYPE integer USING ""RequestType""[1];");
        }
    }
}
