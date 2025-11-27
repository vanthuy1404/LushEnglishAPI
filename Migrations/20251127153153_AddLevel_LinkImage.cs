using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LushEnglishAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLevel_LinkImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WritingScore",
                table: "Results");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "WritingConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LinkImage",
                table: "WritingConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Topics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LinkImage",
                table: "Topics",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Practices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LinkImage",
                table: "Practices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "ChattingConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LinkImage",
                table: "ChattingConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "WritingConfigs");

            migrationBuilder.DropColumn(
                name: "LinkImage",
                table: "WritingConfigs");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "LinkImage",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Practices");

            migrationBuilder.DropColumn(
                name: "LinkImage",
                table: "Practices");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "ChattingConfigs");

            migrationBuilder.DropColumn(
                name: "LinkImage",
                table: "ChattingConfigs");

            migrationBuilder.AddColumn<decimal>(
                name: "WritingScore",
                table: "Results",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
