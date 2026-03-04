using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosqueDonationAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserMosqueAssignmentAdded1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "Mosques",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "Mosques");
        }
    }
}
