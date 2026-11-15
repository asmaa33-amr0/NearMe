using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NearMe.Migrations
{
    /// <inheritdoc />
    public partial class location : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "ProviderProfiles",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "ProviderProfiles",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "ProviderProfiles");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "ProviderProfiles");
        }
    }
}
