using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KariyerNet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateAiExplanationToJobApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CandidateAiExplanation",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CandidateAiExplanation",
                table: "JobApplications");
        }
    }
}
