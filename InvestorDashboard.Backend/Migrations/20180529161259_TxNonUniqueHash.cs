using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class TxNonUniqueHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoTransactions_Hash_Direction",
                table: "CryptoTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoTransactions_Hash_Direction_ExternalId",
                table: "CryptoTransactions",
                columns: new[] { "Hash", "Direction", "ExternalId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoTransactions_Hash_Direction_ExternalId",
                table: "CryptoTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoTransactions_Hash_Direction",
                table: "CryptoTransactions",
                columns: new[] { "Hash", "Direction" },
                unique: true,
                filter: "[Hash] IS NOT NULL");
        }
    }
}
