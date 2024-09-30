// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockUnitCostChangedDomainEvent(TenantId tenantId, Stock stock, Money oldUnitCost, Money newUnitCost) : DomainEventBase
{
    public TenantId TenantId { get; } = tenantId;
    public StockId StockId { get; } = stock.Id;
    public ProductSku Sku { get; } = stock.Sku;
    public Money OldUnitCost { get; } = oldUnitCost;
    public Money NewUnitCost { get; } = newUnitCost;
}