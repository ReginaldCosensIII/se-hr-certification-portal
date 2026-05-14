using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeHrCertificationPortal.Migrations
{
    /// <inheritdoc />
    public partial class CleanupNullRequestTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This removes any NULL elements from the arrays, and if the array itself is null, defaults it to an empty array {}
            migrationBuilder.Sql("UPDATE \"CertificationRequests\" SET \"RequestTypes\" = COALESCE(array_remove(\"RequestTypes\", NULL), '{}');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
