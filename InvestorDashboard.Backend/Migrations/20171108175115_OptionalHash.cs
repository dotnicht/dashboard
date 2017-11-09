using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class OptionalHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoTransactions_Hash",
                table: "CryptoTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "Hash",
                table: "CryptoTransactions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_CryptoTransactions_Hash",
                table: "CryptoTransactions",
                column: "Hash",
                unique: true,
                filter: "[Hash] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoTransactions_Hash",
                table: "CryptoTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "Hash",
                table: "CryptoTransactions",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CryptoTransactions_Hash",
                table: "CryptoTransactions",
                column: "Hash",
                unique: true);
        }
    }
}
