using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceParser.Web.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Sku = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    BaseUrl = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shops", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    ShopId = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    PriceKopeks = table.Column<long>(type: "INTEGER", nullable: true),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    ParsedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    RawPriceText = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceLogs_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PriceLogs_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    ShopId = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductLinks_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductLinks_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceLogs_ProductId",
                table: "PriceLogs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLogs_ShopId",
                table: "PriceLogs",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductLinks_ProductId_ShopId_Url",
                table: "ProductLinks",
                columns: new[] { "ProductId", "ShopId", "Url" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductLinks_ShopId",
                table: "ProductLinks",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Shops_Code",
                table: "Shops",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceLogs");

            migrationBuilder.DropTable(
                name: "ProductLinks");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Shops");
        }
    }
}
