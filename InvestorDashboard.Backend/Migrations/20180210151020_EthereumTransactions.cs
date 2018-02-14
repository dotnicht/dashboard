using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class EthereumTransactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EthereumBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlockHash = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BlockIndex = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EthereumBlocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EthereumTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    From = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    To = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TransactionIndex = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EthereumTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EthereumTransactions_EthereumBlocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "EthereumBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EthereumBlocks_BlockHash",
                table: "EthereumBlocks",
                column: "BlockHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EthereumBlocks_BlockIndex",
                table: "EthereumBlocks",
                column: "BlockIndex",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EthereumTransactions_BlockId",
                table: "EthereumTransactions",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_EthereumTransactions_From",
                table: "EthereumTransactions",
                column: "From");

            migrationBuilder.CreateIndex(
                name: "IX_EthereumTransactions_To",
                table: "EthereumTransactions",
                column: "To");

            migrationBuilder.CreateIndex(
                name: "IX_EthereumTransactions_TransactionHash",
                table: "EthereumTransactions",
                column: "TransactionHash",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EthereumTransactions");

            migrationBuilder.DropTable(
                name: "EthereumBlocks");
        }
    }
}
