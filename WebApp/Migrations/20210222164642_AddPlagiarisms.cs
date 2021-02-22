using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApp.Migrations
{
    public partial class AddPlagiarisms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plagiarisms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProblemId = table.Column<int>(type: "int", nullable: false),
                    Results = table.Column<string>(type: "text", nullable: true),
                    Outdated = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CheckedBy = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    RequestVersion = table.Column<int>(type: "int", nullable: false),
                    CompleteVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plagiarisms", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plagiarisms");
        }
    }
}
