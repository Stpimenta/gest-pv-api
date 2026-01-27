using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace c___Api_Example.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockedPeriodsAndPendingUnlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DesligamentoModel_Usuarios_idMembro",
                table: "DesligamentoModel");

            migrationBuilder.DropForeignKey(
                name: "FK_GastoModel_Caixa_CaixaModelId",
                table: "GastoModel");

            migrationBuilder.DropForeignKey(
                name: "FK_GastoModel_Caixa_IdCaixa",
                table: "GastoModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GastoModel",
                table: "GastoModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DesligamentoModel",
                table: "DesligamentoModel");

            migrationBuilder.RenameTable(
                name: "GastoModel",
                newName: "Gasto");

            migrationBuilder.RenameTable(
                name: "DesligamentoModel",
                newName: "Desligamento");

            migrationBuilder.RenameIndex(
                name: "IX_GastoModel_IdCaixa",
                table: "Gasto",
                newName: "IX_Gasto_IdCaixa");

            migrationBuilder.RenameIndex(
                name: "IX_GastoModel_CaixaModelId",
                table: "Gasto",
                newName: "IX_Gasto_CaixaModelId");

            migrationBuilder.RenameIndex(
                name: "IX_DesligamentoModel_idMembro",
                table: "Desligamento",
                newName: "IX_Desligamento_idMembro");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Gasto",
                table: "Gasto",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Desligamento",
                table: "Desligamento",
                column: "id");

            migrationBuilder.CreateTable(
                name: "BlockedPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    BlockedById = table.Column<int>(type: "integer", nullable: false),
                    BlockedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockedPeriods_Usuarios_BlockedById",
                        column: x => x.BlockedById,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PendingUnlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BlockPeriodId = table.Column<int>(type: "integer", nullable: false),
                    BlockUserId = table.Column<int>(type: "integer", nullable: false),
                    DateUnlocked = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingUnlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingUnlocks_BlockedPeriods_BlockPeriodId",
                        column: x => x.BlockPeriodId,
                        principalTable: "BlockedPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingUnlocks_Usuarios_BlockUserId",
                        column: x => x.BlockUserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockedPeriods_BlockedById",
                table: "BlockedPeriods",
                column: "BlockedById");

            migrationBuilder.CreateIndex(
                name: "IX_PendingUnlocks_BlockPeriodId",
                table: "PendingUnlocks",
                column: "BlockPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingUnlocks_BlockUserId",
                table: "PendingUnlocks",
                column: "BlockUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Desligamento_Usuarios_idMembro",
                table: "Desligamento",
                column: "idMembro",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gasto_Caixa_CaixaModelId",
                table: "Gasto",
                column: "CaixaModelId",
                principalTable: "Caixa",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gasto_Caixa_IdCaixa",
                table: "Gasto",
                column: "IdCaixa",
                principalTable: "Caixa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Desligamento_Usuarios_idMembro",
                table: "Desligamento");

            migrationBuilder.DropForeignKey(
                name: "FK_Gasto_Caixa_CaixaModelId",
                table: "Gasto");

            migrationBuilder.DropForeignKey(
                name: "FK_Gasto_Caixa_IdCaixa",
                table: "Gasto");

            migrationBuilder.DropTable(
                name: "PendingUnlocks");

            migrationBuilder.DropTable(
                name: "BlockedPeriods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Gasto",
                table: "Gasto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Desligamento",
                table: "Desligamento");

            migrationBuilder.RenameTable(
                name: "Gasto",
                newName: "GastoModel");

            migrationBuilder.RenameTable(
                name: "Desligamento",
                newName: "DesligamentoModel");

            migrationBuilder.RenameIndex(
                name: "IX_Gasto_IdCaixa",
                table: "GastoModel",
                newName: "IX_GastoModel_IdCaixa");

            migrationBuilder.RenameIndex(
                name: "IX_Gasto_CaixaModelId",
                table: "GastoModel",
                newName: "IX_GastoModel_CaixaModelId");

            migrationBuilder.RenameIndex(
                name: "IX_Desligamento_idMembro",
                table: "DesligamentoModel",
                newName: "IX_DesligamentoModel_idMembro");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GastoModel",
                table: "GastoModel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DesligamentoModel",
                table: "DesligamentoModel",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_DesligamentoModel_Usuarios_idMembro",
                table: "DesligamentoModel",
                column: "idMembro",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GastoModel_Caixa_CaixaModelId",
                table: "GastoModel",
                column: "CaixaModelId",
                principalTable: "Caixa",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GastoModel_Caixa_IdCaixa",
                table: "GastoModel",
                column: "IdCaixa",
                principalTable: "Caixa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
