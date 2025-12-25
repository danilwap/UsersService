using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsersService.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserChangesFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserChanges_Users_UserId",
                table: "UserChanges");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_UserChanges_Users_UserId",
                table: "UserChanges",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
