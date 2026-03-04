using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosqueDonationAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserMosqueAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedMosqueId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AssignedMosqueId",
                table: "Users",
                column: "AssignedMosqueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Mosques_AssignedMosqueId",
                table: "Users",
                column: "AssignedMosqueId",
                principalTable: "Mosques",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Mosques_AssignedMosqueId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_AssignedMosqueId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AssignedMosqueId",
                table: "Users");
        }
    }
}
