using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biplott.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase4AdminAndContentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_ThemeId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionChoices_QuestionId",
                table: "QuestionChoices");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Traits",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Traits",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Themes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Themes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "QuestionChoices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "QuestionChoices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Traits_IsActive_Code",
                table: "Traits",
                columns: new[] { "IsActive", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_Themes_IsActive_SortOrder",
                table: "Themes",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CreatedAt",
                table: "Questions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuestionType_IsActive",
                table: "Questions",
                columns: new[] { "QuestionType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ThemeId_IsActive",
                table: "Questions",
                columns: new[] { "ThemeId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_UpdatedAt",
                table: "Questions",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionChoices_QuestionId_IsActive",
                table: "QuestionChoices",
                columns: new[] { "QuestionId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Traits_IsActive_Code",
                table: "Traits");

            migrationBuilder.DropIndex(
                name: "IX_Themes_IsActive_SortOrder",
                table: "Themes");

            migrationBuilder.DropIndex(
                name: "IX_Questions_CreatedAt",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_QuestionType_IsActive",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_ThemeId_IsActive",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_UpdatedAt",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionChoices_QuestionId_IsActive",
                table: "QuestionChoices");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Traits");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Traits");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "QuestionChoices");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "QuestionChoices");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ThemeId",
                table: "Questions",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionChoices_QuestionId",
                table: "QuestionChoices",
                column: "QuestionId");
        }
    }
}
