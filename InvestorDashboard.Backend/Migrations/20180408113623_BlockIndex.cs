using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class BlockIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LastBlockIndex",
                table: "CryptoAddresses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StartBlockIndex",
                table: "CryptoAddresses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastBlockIndex",
                table: "CryptoAddresses");

            migrationBuilder.DropColumn(
                name: "StartBlockIndex",
                table: "CryptoAddresses");
        }
    }
}
