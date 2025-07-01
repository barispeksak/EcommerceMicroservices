using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopOrderMicroservice.Migrations
{
    /// <inheritdoc />
    public partial class addedShopCartandPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "shopping_cart_id",
                table: "shop_order",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "total_price",
                table: "shop_order",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "shopping_cart_id",
                table: "shop_order");

            migrationBuilder.DropColumn(
                name: "total_price",
                table: "shop_order");
        }
    }
}
