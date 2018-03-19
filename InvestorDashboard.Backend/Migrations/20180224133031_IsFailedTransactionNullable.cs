using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class IsFailedTransactionNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Failed",
                table: "CryptoTransactions");

            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "CryptoTransactions",
                newName: "Timestamp");

            migrationBuilder.AddColumn<bool>(
                name: "IsFailed",
                table: "CryptoTransactions",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFailed",
                table: "CryptoTransactions");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "CryptoTransactions",
                newName: "TimeStamp");

            migrationBuilder.AddColumn<bool>(
                name: "Failed",
                table: "CryptoTransactions",
                nullable: false,
                defaultValue: false);
        }
    }
}
