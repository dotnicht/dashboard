using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class RemoveTxIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoTransactions_Hash",
                table: "CryptoTransactions");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "CryptoTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoTransactions_Hash_Direction",
                table: "CryptoTransactions",
                columns: new[] { "Hash", "Direction" },
                unique: true,
                filter: "[Hash] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoTransactions_Hash_Direction",
                table: "CryptoTransactions");

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "CryptoTransactions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CryptoTransactions_Hash",
                table: "CryptoTransactions",
                column: "Hash");
        }
    }
}
