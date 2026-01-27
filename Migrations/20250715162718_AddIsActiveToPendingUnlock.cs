using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c___Api_Example.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToPendingUnlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PendingUnlocks",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PendingUnlocks");
        }
    }
}
