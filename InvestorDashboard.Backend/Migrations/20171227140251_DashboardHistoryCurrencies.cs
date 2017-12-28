using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class DashboardHistoryCurrencies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DashboardHistoryItems_Created",
                table: "DashboardHistoryItems");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ExternalId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "Currency",
                table: "DashboardHistoryItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalInvested",
                table: "DashboardHistoryItems",
                type: "decimal(18, 6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalNonInternalCoinsBought",
                table: "DashboardHistoryItems",
                type: "decimal(18, 6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalNonInternalInvested",
                table: "DashboardHistoryItems",
                type: "decimal(18, 6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "CryptoAddresses",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Created",
                table: "ExchangeRates",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardHistoryItems_Created_Currency",
                table: "DashboardHistoryItems",
                columns: new[] { "Created", "Currency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CryptoAddresses_Address",
                table: "CryptoAddresses",
                column: "Address",
                unique: true,
                filter: "[Address] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExchangeRates_Created",
                table: "ExchangeRates");

            migrationBuilder.DropIndex(
                name: "IX_DashboardHistoryItems_Created_Currency",
                table: "DashboardHistoryItems");

            migrationBuilder.DropIndex(
                name: "IX_CryptoAddresses_Address",
                table: "CryptoAddresses");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "DashboardHistoryItems");

            migrationBuilder.DropColumn(
                name: "TotalInvested",
                table: "DashboardHistoryItems");

            migrationBuilder.DropColumn(
                name: "TotalNonInternalCoinsBought",
                table: "DashboardHistoryItems");

            migrationBuilder.DropColumn(
                name: "TotalNonInternalInvested",
                table: "DashboardHistoryItems");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "CryptoAddresses",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DashboardHistoryItems_Created",
                table: "DashboardHistoryItems",
                column: "Created",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ExternalId",
                table: "AspNetUsers",
                column: "ExternalId",
                unique: true,
                filter: "[ExternalId] IS NOT NULL");
        }
    }
}
