// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StockEntityTypeConfiguration
    : TenantAwareEntityTypeConfiguration<Stock>
{
    public override void Configure(EntityTypeBuilder<Stock> builder)
    {
        base.Configure(builder);

        ConfigureStocks(builder);
        ConfigureStockMovements(builder);
        ConfigureStockAdjustments(builder);
    }

    private static void ConfigureStocks(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable("Stocks").HasKey(e => e.Id).IsClustered(false);

        builder.Navigation(e => e.Adjustments).AutoInclude();
        builder.Navigation(e => e.Movements).AutoInclude();

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => StockId.Create(value));

        builder.Property(e => e.TenantId)
            .HasConversion(id => id.Value, value => TenantId.Create(value)).IsRequired();

        builder.Property(e => e.Sku)
            .HasConversion(sku => sku.Value, value => ProductSku.Create(value))
            .IsRequired()
            .HasMaxLength(12);
        builder.HasIndex(nameof(Stock.TenantId), nameof(Stock.Sku)).IsUnique();

        builder.Property(e => e.QuantityOnHand).HasDefaultValue(0).IsRequired();

        builder.Property(e => e.QuantityReserved).HasDefaultValue(0).IsRequired();

        builder.Property(e => e.ReorderThreshold).HasDefaultValue(0).IsRequired();

        builder.Property(e => e.ReorderQuantity).HasDefaultValue(0).IsRequired();

        builder.OwnsOne(
            e => e.UnitCost,
            b =>
            {
                b.Property(e => e.Amount)
                    .HasColumnName("UnitCost")
                    .HasDefaultValue(0)
                    .IsRequired()
                    .HasColumnType("decimal(5,2)");

                b.OwnsOne(
                    e => e.Currency,
                    b =>
                    {
                        b.Property(e => e.Code)
                            .HasColumnName("UnitCostCurrency")
                            .HasDefaultValue("USD")
                            .IsRequired()
                            .HasMaxLength(8);
                    });
            });

        builder.OwnsOne(e => e.Location, b =>
        {
            b.Property(l => l.Aisle).HasColumnName("LocationAisle").IsRequired().HasMaxLength(32);
            b.Property(l => l.Shelf).HasColumnName("LocationShelf").IsRequired().HasMaxLength(32);
            b.Property(l => l.Bin).HasColumnName("LocationBin").IsRequired().HasMaxLength(32);
            b.Property(e => e.Full).HasColumnName("LocationFull").IsRequired().HasMaxLength(128);
            b.HasIndex(e => e.Full);
        });

        builder.Property(e => e.LastRestockedAt).IsRequired(false);

        builder.OwnsOneAuditState();
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());

        builder.Metadata.FindNavigation(nameof(Stock.Movements)).SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Stock.Adjustments)).SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureStockMovements(EntityTypeBuilder<Stock> builder)
    {
        builder.OwnsMany(
            e => e.Movements,
            b =>
            {
                b.ToTable("StockMovements");
                b.WithOwner().HasForeignKey("StockId");
                b.HasKey("Id", "StockId");

                b.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasConversion(id => id.Value, value => StockMovementId.Create(value));

                b.Property(e => e.Quantity)
                    .IsRequired();

                b.Property(e => e.Type)
                    .HasConversion(new EnumerationConverter<StockMovementType>())
                    .IsRequired();

                b.Property(e => e.Reason)
                    .IsRequired(false)
                    .HasMaxLength(1024);

                b.Property(e => e.Timestamp)
                    .IsRequired();
            });
    }

    private static void ConfigureStockAdjustments(EntityTypeBuilder<Stock> builder)
    {
        builder.OwnsMany(
            e => e.Adjustments,
            b =>
            {
                b.ToTable("StockAdjustments");
                b.WithOwner().HasForeignKey("StockId");
                b.HasKey("Id", "StockId");

                b.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasConversion(id => id.Value, value => StockAdjustmentId.Create(value));

                b.Property(e => e.QuantityChange).HasDefaultValue(0).IsRequired(false);

                b.OwnsOne(
                    e => e.OldUnitCost,
                    b =>
                    {
                        b.Property(e => e.Amount)
                            .HasColumnName("OldUnitCost")
                            .HasDefaultValue(0)
                            .IsRequired()
                            .HasColumnType("decimal(5,2)");

                        b.OwnsOne(
                            e => e.Currency,
                            b =>
                            {
                                b.Property(e => e.Code)
                                    .HasColumnName("OldUnitCostCurrency")
                                    .HasDefaultValue("USD")
                                    .IsRequired()
                                    .HasMaxLength(8);
                            });
                    });

                b.OwnsOne(
                    e => e.NewUnitCost,
                    b =>
                    {
                        b.Property(e => e.Amount)
                            .HasColumnName("NewUnitCost")
                            .HasDefaultValue(0)
                            .IsRequired()
                            .HasColumnType("decimal(5,2)");

                        b.OwnsOne(
                            e => e.Currency,
                            b =>
                            {
                                b.Property(e => e.Code)
                                    .HasColumnName("NewUnitCostCurrency")
                                    .HasDefaultValue("USD")
                                    .IsRequired()
                                    .HasMaxLength(8);
                            });
                    });

                b.Property(e => e.Reason)
                    .IsRequired()
                    .HasMaxLength(1024);

                b.Property(e => e.Timestamp).IsRequired();
            });
    }
}