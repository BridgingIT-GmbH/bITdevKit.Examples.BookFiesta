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

public class StockSnapshotEntityTypeConfiguration : TenantAwareEntityTypeConfiguration<StockSnapshot>
{
    public override void Configure(EntityTypeBuilder<StockSnapshot> builder)
    {
        base.Configure(builder);

        builder.ToTable("StockSnapshots").HasKey(e => e.Id).IsClustered(false);

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => StockSnapshotId.Create(value));

        builder.Property(e => e.TenantId)
            .HasConversion(id => id.Value, value => TenantId.Create(value))
            .IsRequired();

        builder.Property(e => e.StockId)
            .HasConversion(id => id.Value, value => StockId.Create(value))
            .IsRequired();

        builder.HasOne<Stock>()
            .WithMany()
            .HasForeignKey(e => e.StockId)
            .IsRequired();

        builder.Property(e => e.Sku)
            .HasConversion(sku => sku.Value, value => ProductSku.Create(value))
            .IsRequired()
            .HasMaxLength(12);
        builder.HasIndex(nameof(StockSnapshot.TenantId), nameof(StockSnapshot.Sku));

        builder.Property(e => e.QuantityOnHand)
            .IsRequired();

        builder.Property(e => e.QuantityReserved)
            .IsRequired();

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

        builder.OwnsOne(e => e.Location,
            locationBuilder =>
            {
                locationBuilder.Property(l => l.Aisle).HasColumnName("LocationAisle").IsRequired().HasMaxLength(50);
                locationBuilder.Property(l => l.Shelf).HasColumnName("LocationShelf").IsRequired().HasMaxLength(50);
                locationBuilder.Property(l => l.Bin).HasColumnName("LocationBin").IsRequired().HasMaxLength(50);
            });

        builder.Property(e => e.Timestamp)
            .IsRequired();

        builder.OwnsOneAuditState();
        //builder.OwnsOne(e => e.AuditState, b => b.ToJson());
    }
}