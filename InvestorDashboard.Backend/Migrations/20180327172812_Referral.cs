using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class Referral : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoTransactions_Hash",
                table: "CryptoTransactions");

            migrationBuilder.AddColumn<bool>(
                name: "IsReferralPaid",
                table: "CryptoTransactions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralCode",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralUserId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CryptoTransactions_Hash",
                table: "CryptoTransactions",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ReferralCode",
                table: "AspNetUsers",
                column: "ReferralCode",
                unique: true,
                filter: "[ReferralCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ReferralUserId",
                table: "AspNetUsers",
                column: "ReferralUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ReferralUserId",
                table: "AspNetUsers",
                column: "ReferralUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ReferralUserId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_CryptoTransactions_Hash",
                table: "CryptoTransactions");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ReferralCode",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ReferralUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsReferralPaid",
                table: "CryptoTransactions");

            migrationBuilder.DropColumn(
                name: "ReferralCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ReferralUserId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoTransactions_Hash",
                table: "CryptoTransactions",
                column: "Hash",
                unique: true,
                filter: "[Hash] IS NOT NULL");
        }
    }
}
