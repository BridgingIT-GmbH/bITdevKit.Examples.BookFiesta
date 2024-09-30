﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.CreateTable(
                name: "__Outbox_DomainEvents",
                schema: "inventory",
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
                name: "__Storage_Documents",
                schema: "inventory",
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
                name: "Stocks",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    QuantityOnHand = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    QuantityReserved = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReorderThreshold = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReorderQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    UnitCostCurrency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true, defaultValue: "USD"),
                    UnitCost = table.Column<decimal>(type: "decimal(5,2)", nullable: true, defaultValue: 0m),
                    LocationAisle = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    LocationShelf = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    LocationBin = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    LastRestockedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditState_CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AuditState_CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AuditState_CreatedDescription = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    AuditState_UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AuditState_UpdatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AuditState_UpdatedDescription = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    AuditState_UpdatedReasons = table.Column<string>(type: "nvarchar(max)", maxLength: 8192, nullable: true),
                    AuditState_Deactivated = table.Column<bool>(type: "bit", nullable: true),
                    AuditState_DeactivatedReasons = table.Column<string>(type: "nvarchar(max)", maxLength: 8192, nullable: true),
                    AuditState_DeactivatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AuditState_DeactivatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AuditState_DeactivatedDescription = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    AuditState_Deleted = table.Column<bool>(type: "bit", nullable: true),
                    AuditState_DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AuditState_DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AuditState_DeletedReason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    AuditState_DeletedDescription = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Stocks_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "organization",
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustments",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityChange = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    OldUnitCostCurrency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true, defaultValue: "USD"),
                    OldUnitCost = table.Column<decimal>(type: "decimal(5,2)", nullable: true, defaultValue: 0m),
                    NewUnitCostCurrency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true, defaultValue: "USD"),
                    NewUnitCost = table.Column<decimal>(type: "decimal(5,2)", nullable: true, defaultValue: 0m),
                    Reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustments", x => new { x.Id, x.StockId });
                    table.ForeignKey(
                        name: "FK_StockAdjustments_Stocks_StockId",
                        column: x => x.StockId,
                        principalSchema: "inventory",
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => new { x.Id, x.StockId });
                    table.ForeignKey(
                        name: "FK_StockMovements_Stocks_StockId",
                        column: x => x.StockId,
                        principalSchema: "inventory",
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockSnapshots",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    QuantityOnHand = table.Column<int>(type: "int", nullable: false),
                    QuantityReserved = table.Column<int>(type: "int", nullable: false),
                    UnitCostCurrency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true, defaultValue: "USD"),
                    UnitCost = table.Column<decimal>(type: "decimal(5,2)", nullable: true, defaultValue: 0m),
                    LocationAisle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LocationShelf = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LocationBin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditState_CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AuditState_CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AuditState_CreatedDescription = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    AuditState_UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AuditState_UpdatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AuditState_UpdatedDescription = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    AuditState_UpdatedReasons = table.Column<string>(type: "nvarchar(max)", maxLength: 8192, nullable: true),
                    AuditState_Deactivated = table.Column<bool>(type: "bit", nullable: true),
                    AuditState_DeactivatedReasons = table.Column<string>(type: "nvarchar(max)", maxLength: 8192, nullable: true),
                    AuditState_DeactivatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AuditState_DeactivatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AuditState_DeactivatedDescription = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    AuditState_Deleted = table.Column<bool>(type: "bit", nullable: true),
                    AuditState_DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AuditState_DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AuditState_DeletedReason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    AuditState_DeletedDescription = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSnapshots", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_StockSnapshots_Stocks_StockId",
                        column: x => x.StockId,
                        principalSchema: "inventory",
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockSnapshots_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "organization",
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_DomainEvents_CreatedDate",
                schema: "inventory",
                table: "__Outbox_DomainEvents",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_DomainEvents_EventId",
                schema: "inventory",
                table: "__Outbox_DomainEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_DomainEvents_ProcessedDate",
                schema: "inventory",
                table: "__Outbox_DomainEvents",
                column: "ProcessedDate");

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_DomainEvents_Type",
                schema: "inventory",
                table: "__Outbox_DomainEvents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX___Storage_Documents_PartitionKey",
                schema: "inventory",
                table: "__Storage_Documents",
                column: "PartitionKey");

            migrationBuilder.CreateIndex(
                name: "IX___Storage_Documents_RowKey",
                schema: "inventory",
                table: "__Storage_Documents",
                column: "RowKey");

            migrationBuilder.CreateIndex(
                name: "IX___Storage_Documents_Type",
                schema: "inventory",
                table: "__Storage_Documents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustments_StockId",
                schema: "inventory",
                table: "StockAdjustments",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_StockId",
                schema: "inventory",
                table: "StockMovements",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_TenantId",
                schema: "inventory",
                table: "Stocks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_TenantId_Sku",
                schema: "inventory",
                table: "Stocks",
                columns: new[] { "TenantId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockSnapshots_StockId",
                schema: "inventory",
                table: "StockSnapshots",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_StockSnapshots_TenantId",
                schema: "inventory",
                table: "StockSnapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StockSnapshots_TenantId_Sku",
                schema: "inventory",
                table: "StockSnapshots",
                columns: new[] { "TenantId", "Sku" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__Outbox_DomainEvents",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "__Storage_Documents",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "StockAdjustments",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "StockMovements",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "StockSnapshots",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "Stocks",
                schema: "inventory");
        }
    }
}