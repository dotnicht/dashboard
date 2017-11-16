using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class ClickId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationRequest",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "IsNotified",
                table: "CryptoTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ClickId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNotified",
                table: "CryptoTransactions");

            migrationBuilder.DropColumn(
                name: "ClickId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "RegistrationRequest",
                table: "AspNetUsers",
                nullable: true);
        }
    }
}
