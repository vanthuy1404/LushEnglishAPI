using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LushEnglishAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDailyLoginStreak_UniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDailyLoginStreaks_UserId",
                table: "UserDailyLoginStreaks");

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyLoginStreaks_UserId_ActivityDate",
                table: "UserDailyLoginStreaks",
                columns: new[] { "UserId", "ActivityDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDailyLoginStreaks_UserId_ActivityDate",
                table: "UserDailyLoginStreaks");

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyLoginStreaks_UserId",
                table: "UserDailyLoginStreaks",
                column: "UserId");
        }
    }
}
