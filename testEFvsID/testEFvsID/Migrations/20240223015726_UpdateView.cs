using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace testEFvsID.Migrations
{
    public partial class UpdateView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "View",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "View",
                table: "Posts");
        }
    }
}
