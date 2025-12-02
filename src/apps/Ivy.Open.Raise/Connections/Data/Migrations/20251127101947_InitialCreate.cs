using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivy.Open.Raise.Connections.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration is intentionally empty.
            // It represents the existing database schema that was created before EF migrations were enabled.
            // The database already has all the tables, so we don't need to create them again.
            //
            // To initialize migrations on an existing database:
            // 1. Run: dotnet ef database update InitialCreate
            //    This will create the __EFMigrationsHistory table and mark this migration as applied.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration is intentionally empty.
            // Rolling back would require dropping the entire database schema.
        }
    }
}
