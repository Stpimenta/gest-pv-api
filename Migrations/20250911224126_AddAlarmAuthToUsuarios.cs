using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c___Api_Example.Migrations
{
    /// <inheritdoc />
    public partial class AddAlarmAuthToUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "Usuarios");

            migrationBuilder.AddColumn<bool>(
                name: "alarmAuth",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "alarmAuth",
                table: "Usuarios");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
