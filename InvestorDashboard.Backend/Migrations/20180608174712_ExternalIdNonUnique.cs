using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class ExternalIdNonUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoTransactions_ExternalId",
                table: "CryptoTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoTransactions_ExternalId",
                table: "CryptoTransactions",
                column: "ExternalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoTransactions_ExternalId",
                table: "CryptoTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoTransactions_ExternalId",
                table: "CryptoTransactions",
                column: "ExternalId",
                unique: true,
                filter: "[ExternalId] IS NOT NULL");
        }
    }
}
