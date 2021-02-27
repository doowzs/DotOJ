using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class AddBonusAndDecay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasScoreBonus",
                table: "Contests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasScoreDecay",
                table: "Contests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsScoreDecayLinear",
                table: "Contests",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScoreBonusPercentage",
                table: "Contests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScoreBonusTime",
                table: "Contests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScoreDecayPercentage",
                table: "Contests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScoreDecayTime",
                table: "Contests",
                type: "datetime(6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasScoreBonus",
                table: "Contests");

            migrationBuilder.DropColumn(
                name: "HasScoreDecay",
                table: "Contests");

            migrationBuilder.DropColumn(
                name: "IsScoreDecayLinear",
                table: "Contests");

            migrationBuilder.DropColumn(
                name: "ScoreBonusPercentage",
                table: "Contests");

            migrationBuilder.DropColumn(
                name: "ScoreBonusTime",
                table: "Contests");

            migrationBuilder.DropColumn(
                name: "ScoreDecayPercentage",
                table: "Contests");

            migrationBuilder.DropColumn(
                name: "ScoreDecayTime",
                table: "Contests");
        }
    }
}
