using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KariyerNet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiEvaluationFieldsToJobApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AiEvaluatedAt",
                table: "JobApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiExplanation",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MatchScore",
                table: "JobApplications",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiEvaluatedAt",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "AiExplanation",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "MatchScore",
                table: "JobApplications");
        }
    }
}
