using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookAuthors_BookId",
                schema: "catalog",
                table: "BookAuthors");

            migrationBuilder.DropIndex(
                name: "IX_AuthorBooks_AuthorId",
                schema: "catalog",
                table: "AuthorBooks");

            migrationBuilder.CreateIndex(
                name: "IX_BookAuthors_BookId_AuthorId",
                schema: "catalog",
                table: "BookAuthors",
                columns: new[] { "BookId", "AuthorId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorBooks_AuthorId_BookId",
                schema: "catalog",
                table: "AuthorBooks",
                columns: new[] { "AuthorId", "BookId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookAuthors_BookId_AuthorId",
                schema: "catalog",
                table: "BookAuthors");

            migrationBuilder.DropIndex(
                name: "IX_AuthorBooks_AuthorId_BookId",
                schema: "catalog",
                table: "AuthorBooks");

            migrationBuilder.CreateIndex(
                name: "IX_BookAuthors_BookId",
                schema: "catalog",
                table: "BookAuthors",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorBooks_AuthorId",
                schema: "catalog",
                table: "AuthorBooks",
                column: "AuthorId");
        }
    }
}
