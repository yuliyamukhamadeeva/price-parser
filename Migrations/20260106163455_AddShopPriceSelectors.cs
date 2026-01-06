using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceParser.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddShopPriceSelectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PriceSelectors",
                table: "Shops",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceSelectors",
                table: "Shops");
        }
    }
}
