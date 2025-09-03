using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace testEFvsID.Migrations
{
    public partial class UpdatePost2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FlashNew",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HotNew",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Slider",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlashNew",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "HotNew",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Slider",
                table: "Posts");
        }
    }
}
