using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class IntegerRefactoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BonusPercentage",
                table: "DashboardHistoryItems");

            migrationBuilder.DropColumn(
                name: "IsTokenSaleDisabled",
                table: "DashboardHistoryItems");

            migrationBuilder.DropColumn(
                name: "TokenPrice",
                table: "DashboardHistoryItems");

            migrationBuilder.DropColumn(
                name: "TotalCoins",
                table: "DashboardHistoryItems");

            migrationBuilder.DropColumn(
                name: "TotalCoinsBought",
                table: "DashboardHistoryItems");

            migrationBuilder.DropColumn(
                name: "TotalNonInternalCoinsBought",
                table: "DashboardHistoryItems");

            migrationBuilder.DropColumn(
                name: "TotalNonInternalUsdInvested",
                table: "DashboardHistoryItems");

            migrationBuilder.DropColumn(
                name: "TotalUsdInvested",
                table: "DashboardHistoryItems");

            migrationBuilder.DropColumn(
                name: "BonusPercentage",
                table: "CryptoTransactions");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "CryptoTransactions");

            migrationBuilder.DropColumn(
                name: "TokenPrice",
                table: "CryptoTransactions");

            migrationBuilder.AddColumn<long>(
                name: "TotalCoinsBoughts",
                table: "DashboardHistoryItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TotalNonInternalCoinsBoughts",
                table: "DashboardHistoryItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "Amount",
                table: "CryptoTransactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18, 6)");

            migrationBuilder.AlterColumn<long>(
                name: "BonusBalance",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18, 6)");

            migrationBuilder.AlterColumn<long>(
                name: "Balance",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18, 6)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalCoinsBoughts",
                table: "DashboardHistoryItems");

            migrationBuilder.DropColumn(
                name: "TotalNonInternalCoinsBoughts",
                table: "DashboardHistoryItems");

            migrationBuilder.AddColumn<decimal>(
                name: "BonusPercentage",
                table: "DashboardHistoryItems",
                type: "decimal(18, 6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsTokenSaleDisabled",
                table: "DashboardHistoryItems",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TokenPrice",
                table: "DashboardHistoryItems",
                type: "decimal(18, 6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCoins",
                table: "DashboardHistoryItems",
                type: "decimal(18, 6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCoinsBought",
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
                name: "TotalNonInternalUsdInvested",
                table: "DashboardHistoryItems",
                type: "decimal(18, 6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalUsdInvested",
                table: "DashboardHistoryItems",
                type: "decimal(18, 6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "CryptoTransactions",
                type: "decimal(18, 6)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BonusPercentage",
                table: "CryptoTransactions",
                type: "decimal(18, 6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "CryptoTransactions",
                type: "decimal(18, 6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TokenPrice",
                table: "CryptoTransactions",
                type: "decimal(18, 6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "BonusBalance",
                table: "AspNetUsers",
                type: "decimal(18, 6)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "AspNetUsers",
                type: "decimal(18, 6)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
