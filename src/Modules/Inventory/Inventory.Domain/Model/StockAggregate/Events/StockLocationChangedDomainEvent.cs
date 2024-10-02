// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;

public class StockLocationChangedDomainEvent(Stock stock, StorageLocation newLocation) : DomainEventBase
{
    public TenantId TenantId { get; } = stock.TenantId;
    public StockId StockId { get; } = stock.Id;
    public ProductSku Sku { get; } = stock.Sku;
    public StorageLocation NewLocation { get; } = newLocation;
}