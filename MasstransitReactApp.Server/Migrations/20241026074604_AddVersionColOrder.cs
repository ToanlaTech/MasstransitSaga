using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasstransitReactApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionColOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Order",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Order");
        }
    }
}
