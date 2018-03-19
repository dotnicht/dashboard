using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class RawData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RawBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawBlocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RawTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Hash = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawTransactions_RawBlocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "RawBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RawParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Hash = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Index = table.Column<long>(type: "bigint", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawParts_RawTransactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "RawTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RawBlocks_Hash",
                table: "RawBlocks",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawBlocks_Index_Currency",
                table: "RawBlocks",
                columns: new[] { "Index", "Currency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawParts_Address",
                table: "RawParts",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_RawParts_Hash",
                table: "RawParts",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_RawParts_TransactionId",
                table: "RawParts",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RawTransactions_BlockId",
                table: "RawTransactions",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_RawTransactions_Hash",
                table: "RawTransactions",
                column: "Hash",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawParts");

            migrationBuilder.DropTable(
                name: "RawTransactions");

            migrationBuilder.DropTable(
                name: "RawBlocks");
        }
    }
}
