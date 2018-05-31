﻿using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Migrations
{
    public partial class TxIndexAddressIdUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoTransactions_Hash_Direction_ExternalId_CryptoAddressId",
                table: "CryptoTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoTransactions_Hash_Direction_ExternalId_CryptoAddressId",
                table: "CryptoTransactions",
                columns: new[] { "Hash", "Direction", "ExternalId", "CryptoAddressId" },
                unique: true,
                filter: "[Hash] IS NOT NULL AND [ExternalId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoTransactions_Hash_Direction_ExternalId_CryptoAddressId",
                table: "CryptoTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoTransactions_Hash_Direction_ExternalId_CryptoAddressId",
                table: "CryptoTransactions",
                columns: new[] { "Hash", "Direction", "ExternalId", "CryptoAddressId" });
        }
    }
}
