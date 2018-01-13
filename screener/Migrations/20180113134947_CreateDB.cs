using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace screener.Migrations
{
    public partial class CreateDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "circuitBreaker",
                columns: table => new
                {
                    nseSymbol = table.Column<string>(nullable: false),
                    date = table.Column<DateTime>(nullable: false),
                    series = table.Column<string>(nullable: false),
                    high_low = table.Column<char>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_circuitBreaker", x => new { x.nseSymbol, x.date, x.series, x.high_low });
                });

            migrationBuilder.CreateTable(
                name: "companyInformation",
                columns: table => new
                {
                    series = table.Column<string>(nullable: false),
                    isinNumber = table.Column<string>(nullable: false),
                    symbol = table.Column<string>(nullable: false),
                    companyName = table.Column<string>(nullable: false),
                    dateOfListing = table.Column<DateTime>(nullable: false),
                    faceValue = table.Column<decimal>(nullable: false),
                    industry = table.Column<string>(nullable: true),
                    marketLot = table.Column<int>(nullable: false),
                    paidUpvalue = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companyInformation", x => new { x.series, x.isinNumber, x.symbol });
                });

            migrationBuilder.CreateTable(
                name: "monthlyStockStats",
                columns: table => new
                {
                    symbol = table.Column<string>(nullable: false),
                    year = table.Column<int>(nullable: false),
                    InternalData = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monthlyStockStats", x => new { x.symbol, x.year });
                });

            migrationBuilder.CreateTable(
                name: "portfolioInformation",
                columns: table => new
                {
                    isinNumber = table.Column<string>(nullable: false),
                    series = table.Column<string>(nullable: false),
                    symbol = table.Column<string>(nullable: false),
                    buyDate = table.Column<DateTime>(nullable: false),
                    buyPrice = table.Column<decimal>(nullable: false),
                    notes = table.Column<string>(nullable: true),
                    userName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolioInformation", x => new { x.isinNumber, x.series, x.symbol });
                });

            migrationBuilder.CreateTable(
                name: "sectorInformation",
                columns: table => new
                {
                    date = table.Column<DateTime>(nullable: false),
                    industry = table.Column<string>(nullable: false),
                    change = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sectorInformation", x => new { x.date, x.industry });
                });

            migrationBuilder.CreateTable(
                name: "stockData",
                columns: table => new
                {
                    isinNumber = table.Column<string>(nullable: false),
                    date = table.Column<DateTime>(nullable: false),
                    series = table.Column<string>(nullable: false),
                    close = table.Column<decimal>(nullable: false),
                    deliverableQty = table.Column<long>(nullable: false),
                    deliveryPercentage = table.Column<decimal>(nullable: false),
                    high = table.Column<decimal>(nullable: false),
                    lastPrice = table.Column<decimal>(nullable: false),
                    low = table.Column<decimal>(nullable: false),
                    open = table.Column<decimal>(nullable: false),
                    prevClose = table.Column<decimal>(nullable: false),
                    symbol = table.Column<string>(nullable: false),
                    totalTradedQty = table.Column<long>(nullable: false),
                    totalTradedValue = table.Column<decimal>(nullable: false),
                    totalTrades = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stockData", x => new { x.isinNumber, x.date, x.series });
                });

            migrationBuilder.CreateTable(
                name: "watchList",
                columns: table => new
                {
                    isinNumber = table.Column<string>(nullable: false),
                    series = table.Column<string>(nullable: false),
                    symbol = table.Column<string>(nullable: false),
                    notes = table.Column<string>(nullable: true),
                    userName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_watchList", x => new { x.isinNumber, x.series, x.symbol });
                });

            migrationBuilder.CreateIndex(
                name: "IX_circuitBreaker_date",
                table: "circuitBreaker",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_circuitBreaker_nseSymbol",
                table: "circuitBreaker",
                column: "nseSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_companyInformation_symbol_isinNumber_series",
                table: "companyInformation",
                columns: new[] { "symbol", "isinNumber", "series" });

            migrationBuilder.CreateIndex(
                name: "IX_monthlyStockStats_symbol",
                table: "monthlyStockStats",
                column: "symbol");

            migrationBuilder.CreateIndex(
                name: "IX_monthlyStockStats_year",
                table: "monthlyStockStats",
                column: "year");

            migrationBuilder.CreateIndex(
                name: "IX_portfolioInformation_isinNumber_series_symbol",
                table: "portfolioInformation",
                columns: new[] { "isinNumber", "series", "symbol" });

            migrationBuilder.CreateIndex(
                name: "IX_sectorInformation_date",
                table: "sectorInformation",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_sectorInformation_industry",
                table: "sectorInformation",
                column: "industry");

            migrationBuilder.CreateIndex(
                name: "IX_stockData_date",
                table: "stockData",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_stockData_symbol_series",
                table: "stockData",
                columns: new[] { "symbol", "series" });

            migrationBuilder.CreateIndex(
                name: "IX_watchList_isinNumber_series_symbol",
                table: "watchList",
                columns: new[] { "isinNumber", "series", "symbol" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "circuitBreaker");

            migrationBuilder.DropTable(
                name: "companyInformation");

            migrationBuilder.DropTable(
                name: "monthlyStockStats");

            migrationBuilder.DropTable(
                name: "portfolioInformation");

            migrationBuilder.DropTable(
                name: "sectorInformation");

            migrationBuilder.DropTable(
                name: "stockData");

            migrationBuilder.DropTable(
                name: "watchList");
        }
    }
}
