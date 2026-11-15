using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NearMe.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "ProviderProfiles",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contact",
                table: "ProviderProfiles");
        }
    }
}
