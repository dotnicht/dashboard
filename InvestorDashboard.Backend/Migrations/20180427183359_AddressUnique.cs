using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class AddressUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoAddresses_Address",
                table: "CryptoAddresses");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "CryptoAddresses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CryptoAddresses_Currency_Type_IsDisabled_UserId",
                table: "CryptoAddresses",
                columns: new[] { "Currency", "Type", "IsDisabled", "UserId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoAddresses_Currency_Type_IsDisabled_UserId",
                table: "CryptoAddresses");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "CryptoAddresses",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CryptoAddresses_Address",
                table: "CryptoAddresses",
                column: "Address",
                unique: true,
                filter: "[Address] IS NOT NULL");
        }
    }
}
