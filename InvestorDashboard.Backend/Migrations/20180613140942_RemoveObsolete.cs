using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class RemoveObsolete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInvestor",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "KycBonus",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TokensAvailableForTransfer",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UseNewBonusSystem",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInvestor",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "KycBonus",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TokensAvailableForTransfer",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseNewBonusSystem",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);
        }
    }
}
