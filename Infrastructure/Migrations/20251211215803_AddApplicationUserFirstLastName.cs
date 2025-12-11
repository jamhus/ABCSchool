using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationUserFirstLastName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                schema: "Multitenancy",
                table: "Tenants",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "Multitenancy",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "Multitenancy",
                table: "Tenants");

            migrationBuilder.RenameColumn(
                name: "LastName",
                schema: "Multitenancy",
                table: "Tenants",
                newName: "FullName");
        }
    }
}
