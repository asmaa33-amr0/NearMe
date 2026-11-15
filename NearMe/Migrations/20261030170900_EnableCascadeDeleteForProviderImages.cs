using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NearMe.Migrations
{
    /// <inheritdoc />
    public partial class EnableCascadeDeleteForProviderImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProviderImages_ProviderProfiles_ProviderProfileId",
                table: "ProviderImages");

           
            migrationBuilder.AddForeignKey(
                name: "FK_ProviderImages_ProviderProfiles_ProviderProfileId",
                table: "ProviderImages",
                column: "ProviderProfileId",
                principalTable: "ProviderProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProviderImages_ProviderProfiles_ProviderProfileId",
                table: "ProviderImages");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "ProviderProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderImages_ProviderProfiles_ProviderProfileId",
                table: "ProviderImages",
                column: "ProviderProfileId",
                principalTable: "ProviderProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
