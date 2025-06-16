using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MersinArabaKiralama.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignInfoToRental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CampaignInfo",
                table: "Rentals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CampaignInfo",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Customers");
        }
    }
}
