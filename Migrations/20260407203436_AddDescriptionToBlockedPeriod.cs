using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c___Api_Example.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToBlockedPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlComprovante",
                table: "Gasto");

            migrationBuilder.DropColumn(
                name: "UrlEnvelope",
                table: "Contribuicao");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "BlockedPeriods",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "BlockedPeriods");

            migrationBuilder.AddColumn<string>(
                name: "UrlComprovante",
                table: "Gasto",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlEnvelope",
                table: "Contribuicao",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
