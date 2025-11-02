using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tienda.src.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCartTotalFieldsToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalSavedAmount",
                table: "Carts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalUniqueItemsCount",
                table: "Carts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalSavedAmount",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "TotalUniqueItemsCount",
                table: "Carts");
        }
    }
}