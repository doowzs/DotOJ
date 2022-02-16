using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class newReview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeSpecification",
                table: "SubmissionReviews",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SpaceComplexity",
                table: "SubmissionReviews",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TimeComplexity",
                table: "SubmissionReviews",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeSpecification",
                table: "SubmissionReviews");

            migrationBuilder.DropColumn(
                name: "SpaceComplexity",
                table: "SubmissionReviews");

            migrationBuilder.DropColumn(
                name: "TimeComplexity",
                table: "SubmissionReviews");
        }
    }
}
