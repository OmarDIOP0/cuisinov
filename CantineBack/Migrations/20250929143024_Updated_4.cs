using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CantineBack.Migrations
{
    /// <inheritdoc />
    public partial class Updated_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EntrepriseCode",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntrepriseCode",
                table: "Users");
        }
    }
}
