using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LushEnglishAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelationship_Practice_Chatting_Writing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChattingConfigs_Practices_PracticeId",
                table: "ChattingConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Practices_PracticeId",
                table: "Results");

            migrationBuilder.DropForeignKey(
                name: "FK_WritingConfigs_Practices_PracticeId",
                table: "WritingConfigs");

            migrationBuilder.DropIndex(
                name: "IX_WritingConfigs_PracticeId",
                table: "WritingConfigs");

            migrationBuilder.DropIndex(
                name: "IX_ChattingConfigs_PracticeId",
                table: "ChattingConfigs");

            migrationBuilder.DropColumn(
                name: "PracticeType",
                table: "Practices");

            migrationBuilder.RenameColumn(
                name: "PracticeId",
                table: "WritingConfigs",
                newName: "TopicId");

            migrationBuilder.RenameColumn(
                name: "PracticeId",
                table: "ChattingConfigs",
                newName: "TopicId");

            migrationBuilder.AlterColumn<Guid>(
                name: "PracticeId",
                table: "Results",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "TargetId",
                table: "Results",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_WritingConfigs_TopicId",
                table: "WritingConfigs",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_ChattingConfigs_TopicId",
                table: "ChattingConfigs",
                column: "TopicId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChattingConfigs_Topics_TopicId",
                table: "ChattingConfigs",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Practices_PracticeId",
                table: "Results",
                column: "PracticeId",
                principalTable: "Practices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WritingConfigs_Topics_TopicId",
                table: "WritingConfigs",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChattingConfigs_Topics_TopicId",
                table: "ChattingConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Practices_PracticeId",
                table: "Results");

            migrationBuilder.DropForeignKey(
                name: "FK_WritingConfigs_Topics_TopicId",
                table: "WritingConfigs");

            migrationBuilder.DropIndex(
                name: "IX_WritingConfigs_TopicId",
                table: "WritingConfigs");

            migrationBuilder.DropIndex(
                name: "IX_ChattingConfigs_TopicId",
                table: "ChattingConfigs");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "Results");

            migrationBuilder.RenameColumn(
                name: "TopicId",
                table: "WritingConfigs",
                newName: "PracticeId");

            migrationBuilder.RenameColumn(
                name: "TopicId",
                table: "ChattingConfigs",
                newName: "PracticeId");

            migrationBuilder.AlterColumn<Guid>(
                name: "PracticeId",
                table: "Results",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PracticeType",
                table: "Practices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WritingConfigs_PracticeId",
                table: "WritingConfigs",
                column: "PracticeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChattingConfigs_PracticeId",
                table: "ChattingConfigs",
                column: "PracticeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChattingConfigs_Practices_PracticeId",
                table: "ChattingConfigs",
                column: "PracticeId",
                principalTable: "Practices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Practices_PracticeId",
                table: "Results",
                column: "PracticeId",
                principalTable: "Practices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WritingConfigs_Practices_PracticeId",
                table: "WritingConfigs",
                column: "PracticeId",
                principalTable: "Practices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
