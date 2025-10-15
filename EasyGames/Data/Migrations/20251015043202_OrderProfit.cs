using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrderProfit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnitBuyPrice",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitBuyPrice",
                table: "OrderItems");
        }
    }
}
