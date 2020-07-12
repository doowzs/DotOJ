using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Judge1.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    BeginTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentNotices",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    AssignmentId = table.Column<int>(nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentNotices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentNotices_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentRegistrations",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    AssignmentId = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    IsParticipant = table.Column<bool>(nullable: false),
                    IsAssignmentManager = table.Column<bool>(nullable: false),
                    statistics = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentRegistrations", x => new { x.UserId, x.AssignmentId });
                    table.ForeignKey(
                        name: "FK_AssignmentRegistrations_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentRegistrations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Problems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    InputFormat = table.Column<string>(type: "text", nullable: false),
                    OutputFormat = table.Column<string>(type: "text", nullable: false),
                    FootNote = table.Column<string>(type: "text", nullable: true),
                    TimeLimit = table.Column<double>(nullable: false),
                    MemoryLimit = table.Column<double>(nullable: false),
                    HasSpecialJudge = table.Column<bool>(nullable: false),
                    SpecialJudgeProgram = table.Column<string>(type: "text", nullable: true),
                    HasHacking = table.Column<bool>(nullable: false),
                    StandardProgram = table.Column<string>(type: "text", nullable: true),
                    ValidatorProgram = table.Column<string>(type: "text", nullable: false),
                    SampleCases = table.Column<string>(type: "text", nullable: false),
                    TestCases = table.Column<string>(type: "text", nullable: true),
                    AssignmentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Problems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Problems_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Problems_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    ProblemId = table.Column<int>(nullable: false),
                    AssignmentId = table.Column<int>(nullable: false),
                    program = table.Column<string>(type: "text", nullable: false),
                    Verdict = table.Column<int>(nullable: false),
                    JudgedAt = table.Column<DateTime>(nullable: false),
                    IsHacked = table.Column<bool>(nullable: false),
                    HackedAt = table.Column<DateTime>(nullable: false),
                    HackerId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Submissions_AspNetUsers_HackerId",
                        column: x => x.HackerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Submissions_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Submissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Hacks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    ProblemId = table.Column<int>(nullable: false),
                    AssignmentId = table.Column<int>(nullable: false),
                    SubmissionId = table.Column<int>(nullable: false),
                    Input = table.Column<string>(type: "text", nullable: false),
                    IsValid = table.Column<bool>(nullable: false),
                    ValidatedAt = table.Column<DateTime>(nullable: false),
                    IsSuccessful = table.Column<bool>(nullable: false),
                    JudgedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hacks_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Hacks_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Hacks_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Hacks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentNotices_AssignmentId",
                table: "AssignmentNotices",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentRegistrations_AssignmentId",
                table: "AssignmentRegistrations",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Hacks_AssignmentId",
                table: "Hacks",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Hacks_ProblemId",
                table: "Hacks",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Hacks_SubmissionId",
                table: "Hacks",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Hacks_UserId",
                table: "Hacks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Problems_AssignmentId",
                table: "Problems",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Problems_UserId",
                table: "Problems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_AssignmentId",
                table: "Submissions",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_HackerId",
                table: "Submissions",
                column: "HackerId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ProblemId",
                table: "Submissions",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_UserId",
                table: "Submissions",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentNotices");

            migrationBuilder.DropTable(
                name: "AssignmentRegistrations");

            migrationBuilder.DropTable(
                name: "Hacks");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "Problems");

            migrationBuilder.DropTable(
                name: "Assignments");
        }
    }
}
