using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "__Outbox_DomainEvents",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProcessedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK___Outbox_DomainEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "__Outbox_Messages",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProcessedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK___Outbox_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "__Storage_Documents",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    PartitionKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    RowKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK___Storage_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Authors",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonNameTitle = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PersonNameParts = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    PersonNameSuffix = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PersonNameFull = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Biography = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditState = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Isbn = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    IsbnType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    PriceCurrency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true, defaultValue: "USD"),
                    Price = table.Column<decimal>(type: "decimal(5,2)", nullable: true, defaultValue: 0m),
                    PublisherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PublisherName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditState = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditState = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "catalog",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AddressPostalCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    AddressCity = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AddressCountry = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditState = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Publishers",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AddressPostalCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    AddressCity = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AddressCountry = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditState = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthorBooks",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorBooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorBooks_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalSchema: "catalog",
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalSchema: "catalog",
                        principalTable: "Authors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BookAuthors",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookAuthors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookAuthors_Books_BookId",
                        column: x => x.BookId,
                        principalSchema: "catalog",
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookChapters",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookChapters", x => new { x.Id, x.BookId });
                    table.ForeignKey(
                        name: "FK_BookChapters_Books_BookId",
                        column: x => x.BookId,
                        principalSchema: "catalog",
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookCategories",
                schema: "catalog",
                columns: table => new
                {
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCategories", x => new { x.BookId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_BookCategories_Books_BookId",
                        column: x => x.BookId,
                        principalSchema: "catalog",
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "catalog",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookTags",
                schema: "catalog",
                columns: table => new
                {
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookTags", x => new { x.BookId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_BookTags_Books_BookId",
                        column: x => x.BookId,
                        principalSchema: "catalog",
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookTags_Tags_TagsId",
                        column: x => x.TagsId,
                        principalSchema: "catalog",
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_DomainEvents_CreatedDate",
                schema: "catalog",
                table: "__Outbox_DomainEvents",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_DomainEvents_EventId",
                schema: "catalog",
                table: "__Outbox_DomainEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_DomainEvents_ProcessedDate",
                schema: "catalog",
                table: "__Outbox_DomainEvents",
                column: "ProcessedDate");

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_DomainEvents_Type",
                schema: "catalog",
                table: "__Outbox_DomainEvents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_Messages_CreatedDate",
                schema: "catalog",
                table: "__Outbox_Messages",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_Messages_MessageId",
                schema: "catalog",
                table: "__Outbox_Messages",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_Messages_ProcessedDate",
                schema: "catalog",
                table: "__Outbox_Messages",
                column: "ProcessedDate");

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_Messages_Type",
                schema: "catalog",
                table: "__Outbox_Messages",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX___Storage_Documents_PartitionKey",
                schema: "catalog",
                table: "__Storage_Documents",
                column: "PartitionKey");

            migrationBuilder.CreateIndex(
                name: "IX___Storage_Documents_RowKey",
                schema: "catalog",
                table: "__Storage_Documents",
                column: "RowKey");

            migrationBuilder.CreateIndex(
                name: "IX___Storage_Documents_Type",
                schema: "catalog",
                table: "__Storage_Documents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorBooks_AuthorId_BookId",
                schema: "catalog",
                table: "AuthorBooks",
                columns: new[] { "AuthorId", "BookId" });

            migrationBuilder.CreateIndex(
                name: "IX_BookAuthors_BookId_AuthorId",
                schema: "catalog",
                table: "BookAuthors",
                columns: new[] { "BookId", "AuthorId" });

            migrationBuilder.CreateIndex(
                name: "IX_BookCategories_CategoryId",
                schema: "catalog",
                table: "BookCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BookChapters_BookId",
                schema: "catalog",
                table: "BookChapters",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Isbn",
                schema: "catalog",
                table: "Books",
                column: "Isbn",
                unique: true,
                filter: "[Isbn] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BookTags_TagsId",
                schema: "catalog",
                table: "BookTags",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId",
                schema: "catalog",
                table: "Categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                schema: "catalog",
                table: "Customers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Publishers_Email",
                schema: "catalog",
                table: "Publishers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_AuthorId",
                schema: "catalog",
                table: "Tags",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                schema: "catalog",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__Outbox_DomainEvents",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "__Outbox_Messages",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "__Storage_Documents",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "AuthorBooks",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "BookAuthors",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "BookCategories",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "BookChapters",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "BookTags",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Customers",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Publishers",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Books",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Tags",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Authors",
                schema: "catalog");
        }
    }
}
