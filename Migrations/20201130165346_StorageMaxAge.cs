using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Podlonia.Migrations
{
    public partial class StorageMaxAge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DownloadedAt",
                table: "Enclosures",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadedAt",
                table: "Enclosures");
        }
    }
}
