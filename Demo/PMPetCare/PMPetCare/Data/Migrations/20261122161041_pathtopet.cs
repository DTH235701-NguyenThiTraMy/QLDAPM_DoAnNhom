using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMPetCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class pathtopet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Pets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Pets");
        }
    }
}
