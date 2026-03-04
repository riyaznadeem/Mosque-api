using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosqueDonationAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserMosqueAssignmentAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UrduName",
                table: "Mosques",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DonorNameUrdu",
                table: "Donations",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrduName",
                table: "Mosques");

            migrationBuilder.DropColumn(
                name: "DonorNameUrdu",
                table: "Donations");
        }
    }
}
