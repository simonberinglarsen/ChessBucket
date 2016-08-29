using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DemoSite.Migrations
{
    public partial class asdf : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlackElo",
                table: "Games",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventCountry",
                table: "Games",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhiteElo",
                table: "Games",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlackElo",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "EventCountry",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "WhiteElo",
                table: "Games");
        }
    }
}
