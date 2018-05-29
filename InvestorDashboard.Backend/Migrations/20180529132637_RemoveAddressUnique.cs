using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class RemoveAddressUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoAddresses_Currency_Type_IsDisabled_UserId",
                table: "CryptoAddresses");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoAddresses_Currency_Type_IsDisabled_UserId",
                table: "CryptoAddresses",
                columns: new[] { "Currency", "Type", "IsDisabled", "UserId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoAddresses_Currency_Type_IsDisabled_UserId",
                table: "CryptoAddresses");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoAddresses_Currency_Type_IsDisabled_UserId",
                table: "CryptoAddresses",
                columns: new[] { "Currency", "Type", "IsDisabled", "UserId" },
                unique: true);
        }
    }
}
