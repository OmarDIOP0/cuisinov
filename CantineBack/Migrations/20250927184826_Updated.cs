using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CantineBack.Migrations
{
    /// <inheritdoc />
    public partial class Updated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UseActiveDirectoryAuth",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Bureau",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Entreprises",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bureau",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Entreprises");

            migrationBuilder.AddColumn<bool>(
                name: "UseActiveDirectoryAuth",
                table: "Users",
                type: "bit",
                nullable: true);
        }
    }
}
