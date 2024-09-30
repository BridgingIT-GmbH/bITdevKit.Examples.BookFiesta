// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

using System;

[DebuggerDisplay("Id={Id}, Sku={Sku}, Timestamp={Timestamp}")]
[TypedEntityId<Guid>]
public class StockSnapshot : AuditableAggregateRoot<StockSnapshotId>, IConcurrent
{
    private StockSnapshot() { } // Private constructor required by EF Core

    private StockSnapshot(
        TenantId tenantId,
        StockId stockId,
        ProductSku sku,
        int quantityOnHand,
        int quantityReserved,
        Money unitCost,
        StorageLocation location,
        DateTimeOffset timestamp)
    {
        this.TenantId = tenantId;
        this.StockId = stockId;
        this.Sku = sku;
        this.QuantityOnHand = quantityOnHand;
        this.QuantityReserved = quantityReserved;
        this.UnitCost = unitCost;
        this.Location = location;
        this.Timestamp = timestamp;
    }

    public TenantId TenantId { get; private set; }

    public StockId StockId { get; private set; }

    public ProductSku Sku { get; private set; }

    public int QuantityOnHand { get; private set; }

    public int QuantityReserved { get; private set; }

    public Money UnitCost { get; private set; }

    public StorageLocation Location { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public Guid Version { get; set; }

    public static StockSnapshot Create(
        TenantId tenantId,
        StockId stockId,
        ProductSku sku,
        int quantityOnHand,
        int quantityReserved,
        Money unitCost,
        StorageLocation location,
        DateTimeOffset? timestamp = null)
    {
        _ = tenantId ?? throw new DomainRuleException("TenantId cannot be empty.");
        _ = stockId ?? throw new DomainRuleException("StockId cannot be empty.");
        _ = sku ?? throw new DomainRuleException("ProductSku cannot be empty.");
        _ = unitCost ?? throw new DomainRuleException("UnitCost cannot be empty.");
        _ = location ?? throw new DomainRuleException("Location cannot be empty.");

        var snapshot = new StockSnapshot(tenantId, stockId, sku, quantityOnHand, quantityReserved, unitCost, location, timestamp ?? DateTimeOffset.UtcNow);

        snapshot.DomainEvents.Register(new StockSnapshotCreatedDomainEvent(snapshot));

        return snapshot;
    }
}