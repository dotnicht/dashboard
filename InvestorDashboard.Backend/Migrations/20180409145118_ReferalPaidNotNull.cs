using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class ReferalPaidNotNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsReferralPaid",
                table: "CryptoTransactions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "CryptoTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSpent",
                table: "CryptoTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Index",
                table: "CryptoTransactions");

            migrationBuilder.DropColumn(
                name: "IsSpent",
                table: "CryptoTransactions");

            migrationBuilder.AlterColumn<bool>(
                name: "IsReferralPaid",
                table: "CryptoTransactions",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
