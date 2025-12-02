using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivy.Open.Raise.Connections.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStartupNameAndGovId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "startup_name",
                table: "organization_settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "startup_gov_id",
                table: "organization_settings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "startup_name",
                table: "organization_settings");

            migrationBuilder.DropColumn(
                name: "startup_gov_id",
                table: "organization_settings");
        }
    }
}
