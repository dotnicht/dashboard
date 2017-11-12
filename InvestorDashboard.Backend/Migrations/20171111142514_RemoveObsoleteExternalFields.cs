using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class RemoveObsoleteExternalFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivationDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ExternalBitcoinInvestment",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ExternalEthereumInvestment",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActivationDate",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExternalBitcoinInvestment",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExternalEthereumInvestment",
                table: "AspNetUsers",
                nullable: true);
        }
    }
}
