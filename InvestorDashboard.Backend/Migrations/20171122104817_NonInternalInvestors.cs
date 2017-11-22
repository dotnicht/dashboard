using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class NonInternalInvestors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalNonInternalInvestors",
                table: "DashboardHistoryItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalNonInternalUsdInvested",
                table: "DashboardHistoryItems",
                type: "decimal(18, 6)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalNonInternalInvestors",
                table: "DashboardHistoryItems");

            migrationBuilder.DropColumn(
                name: "TotalNonInternalUsdInvested",
                table: "DashboardHistoryItems");
        }
    }
}
