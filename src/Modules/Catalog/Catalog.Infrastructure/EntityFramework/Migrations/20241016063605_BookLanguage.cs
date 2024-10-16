using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class BookLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                schema: "catalog",
                table: "Books",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Language",
                schema: "catalog",
                table: "Books",
                column: "Language");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_Language",
                schema: "catalog",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Language",
                schema: "catalog",
                table: "Books");
        }
    }
}
