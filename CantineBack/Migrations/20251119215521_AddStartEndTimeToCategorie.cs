using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CantineBack.Migrations
{
    /// <inheritdoc />
    public partial class AddStartEndTimeToCategorie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "Categories",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "Categories",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Categories");
        }
    }
}
