using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LushEnglishAPI.Migrations
{
    /// <inheritdoc />
    public partial class add_field_LoginSession_to_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoginSession",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoginSession",
                table: "Users");
        }
    }
}
