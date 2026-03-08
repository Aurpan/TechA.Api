using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechA.Repository.Migrations
{
    /// <inheritdoc />
    public partial class MoveDisplayNameToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "display_name",
                table: "user_profiles");

            migrationBuilder.AddColumn<string>(
                name: "display_name",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "display_name",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "display_name",
                table: "user_profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
