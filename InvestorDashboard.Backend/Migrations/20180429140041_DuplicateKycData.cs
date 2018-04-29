using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class DuplicateKycData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasDuplicateKycData",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IgnoreDuplicateKycData",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasDuplicateKycData",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IgnoreDuplicateKycData",
                table: "AspNetUsers");
        }
    }
}
