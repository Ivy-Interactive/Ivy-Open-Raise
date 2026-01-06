using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivy.Open.Raise.Connections.Data.Migrations
{
    /// <inheritdoc />
    public partial class OnboardingCompleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "onboarding_completed",
                table: "organization_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "onboarding_completed",
                table: "organization_settings");
        }
    }
}
