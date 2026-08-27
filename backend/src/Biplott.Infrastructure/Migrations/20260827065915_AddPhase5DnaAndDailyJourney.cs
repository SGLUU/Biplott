using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biplott.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase5DnaAndDailyJourney : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JourneyId",
                table: "UserQuestionHistories",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DnaResetAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DailyJourneys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GuestSessionToken = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    DailyDate = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    TotalSteps = table.Column<int>(type: "int", nullable: false),
                    ExpectedQuestionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyJourneys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyJourneys_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTraitProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TraitId = table.Column<int>(type: "int", nullable: false),
                    AccumulatedWeight = table.Column<double>(type: "float", nullable: false),
                    SampleCount = table.Column<int>(type: "int", nullable: false),
                    NormalizedScore = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTraitProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTraitProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTraitProfiles_Traits_TraitId",
                        column: x => x.TraitId,
                        principalTable: "Traits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyJourneyAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyJourneyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    ChoiceId = table.Column<int>(type: "int", nullable: false),
                    StepIndex = table.Column<int>(type: "int", nullable: false),
                    QuestionContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChoiceContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subtitle = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyJourneyAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyJourneyAnswers_DailyJourneys_DailyJourneyId",
                        column: x => x.DailyJourneyId,
                        principalTable: "DailyJourneys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyJourneyNumbers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyJourneyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    PoolIndex = table.Column<int>(type: "int", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    DominantTrait = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyJourneyNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyJourneyNumbers_DailyJourneys_DailyJourneyId",
                        column: x => x.DailyJourneyId,
                        principalTable: "DailyJourneys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestionHistories_JourneyId_QuestionId",
                table: "UserQuestionHistories",
                columns: new[] { "JourneyId", "QuestionId" },
                unique: true,
                filter: "[JourneyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DailyJourneyAnswers_DailyJourneyId",
                table: "DailyJourneyAnswers",
                column: "DailyJourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyJourneyNumbers_DailyJourneyId",
                table: "DailyJourneyNumbers",
                column: "DailyJourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyJourneys_GameId",
                table: "DailyJourneys",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyJourneys_GuestSessionToken_GameId_DailyDate",
                table: "DailyJourneys",
                columns: new[] { "GuestSessionToken", "GameId", "DailyDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyJourneys_UserId_GameId_DailyDate",
                table: "DailyJourneys",
                columns: new[] { "UserId", "GameId", "DailyDate" },
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserTraitProfiles_TraitId",
                table: "UserTraitProfiles",
                column: "TraitId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTraitProfiles_UserId",
                table: "UserTraitProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTraitProfiles_UserId_TraitId",
                table: "UserTraitProfiles",
                columns: new[] { "UserId", "TraitId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyJourneyAnswers");

            migrationBuilder.DropTable(
                name: "DailyJourneyNumbers");

            migrationBuilder.DropTable(
                name: "UserTraitProfiles");

            migrationBuilder.DropTable(
                name: "DailyJourneys");

            migrationBuilder.DropIndex(
                name: "IX_UserQuestionHistories_JourneyId_QuestionId",
                table: "UserQuestionHistories");

            migrationBuilder.DropColumn(
                name: "JourneyId",
                table: "UserQuestionHistories");

            migrationBuilder.DropColumn(
                name: "DnaResetAt",
                table: "AspNetUsers");
        }
    }
}
