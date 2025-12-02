using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivy.Open.Raise.Connections.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameStartupStageToStartupStageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_organization_settings_startup_stages_startup_stage",
                table: "organization_settings");

            migrationBuilder.RenameColumn(
                name: "startup_stage",
                table: "organization_settings",
                newName: "startup_stage_id");

            migrationBuilder.RenameIndex(
                name: "IX_organization_settings_startup_stage",
                table: "organization_settings",
                newName: "IX_organization_settings_startup_stage_id");

            migrationBuilder.AddForeignKey(
                name: "FK_organization_settings_startup_stages_startup_stage_id",
                table: "organization_settings",
                column: "startup_stage_id",
                principalTable: "startup_stages",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_organization_settings_startup_stages_startup_stage_id",
                table: "organization_settings");

            migrationBuilder.RenameColumn(
                name: "startup_stage_id",
                table: "organization_settings",
                newName: "startup_stage");

            migrationBuilder.RenameIndex(
                name: "IX_organization_settings_startup_stage_id",
                table: "organization_settings",
                newName: "IX_organization_settings_startup_stage");

            migrationBuilder.AddForeignKey(
                name: "FK_organization_settings_startup_stages_startup_stage",
                table: "organization_settings",
                column: "startup_stage",
                principalTable: "startup_stages",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
