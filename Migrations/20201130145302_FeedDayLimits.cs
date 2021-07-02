using Microsoft.EntityFrameworkCore.Migrations;

namespace Podlonia.Migrations
{
    public partial class FeedDayLimits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DownloadEnclosureMaxAgeDays",
                table: "Feeds",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "StoredEnclosureMaxAgeDays",
                table: "Feeds",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadEnclosureMaxAgeDays",
                table: "Feeds");

            migrationBuilder.DropColumn(
                name: "StoredEnclosureMaxAgeDays",
                table: "Feeds");
        }
    }
}
