using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivy.Open.Raise.Connections.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFinalToDealState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_final",
                table: "deal_states",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_final",
                table: "deal_states");
        }
    }
}
