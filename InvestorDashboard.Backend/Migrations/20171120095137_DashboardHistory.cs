using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class DashboardHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardHistoryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BonusPercentage = table.Column<decimal>(type: "decimal(18, 6)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsTokenSaleDisabled = table.Column<bool>(type: "bit", nullable: false),
                    TokenPrice = table.Column<decimal>(type: "decimal(18, 6)", nullable: false),
                    TotalCoins = table.Column<decimal>(type: "decimal(18, 6)", nullable: false),
                    TotalCoinsBought = table.Column<decimal>(type: "decimal(18, 6)", nullable: false),
                    TotalInvestors = table.Column<int>(type: "int", nullable: false),
                    TotalUsdInvested = table.Column<decimal>(type: "decimal(18, 6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardHistoryItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardHistoryItems_Created",
                table: "DashboardHistoryItems",
                column: "Created",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardHistoryItems");
        }
    }
}
