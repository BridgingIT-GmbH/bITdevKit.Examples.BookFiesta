// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockSnapshotCreatedDomainEvent(StockSnapshot snapshot) : DomainEventBase
{
    public StockSnapshotId Id { get; } = snapshot.Id;
    public TenantId TenantId { get; } = snapshot.TenantId;
    public StockId StockId { get; } = snapshot.StockId;
    public ProductSku Sku { get; } = snapshot.Sku;
    public int QuantityOnHand { get; } = snapshot.QuantityOnHand;
    public int QuantityReserved { get; } = snapshot.QuantityReserved;
    public Money UnitCost { get; } = snapshot.UnitCost;
    public StorageLocation Location { get; } = snapshot.Location;
    public DateTimeOffset SnapshotTimestamp { get; } = snapshot.Timestamp;
}