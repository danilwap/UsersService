using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsersService.Migrations
{
    /// <inheritdoc />
    public partial class fixed_orders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "amount",
                table: "Orders",
                newName: "Amount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Orders",
                newName: "amount");
        }
    }
}
