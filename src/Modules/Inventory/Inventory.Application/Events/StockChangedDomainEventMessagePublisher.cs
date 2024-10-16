// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application.Events;

using BridgingIT.DevKit.Application.Messaging;

public class StockChangedDomainEventMessagePublisher(
    ILoggerFactory loggerFactory,
    IMessageBroker messageBroker)
    : DomainEventHandlerBase<StockChangedDomainEvent>(loggerFactory)
{
    public override bool CanHandle(StockChangedDomainEvent @event)
    {
        return true;
    }

    public override async Task Process(StockChangedDomainEvent @event, CancellationToken cancellationToken)
    {
        await messageBroker.Publish(
            new StockChangedMessage
            {
                TenantId = @event.TenantId,
                StockId = @event.StockId,
                Sku = @event.Sku,
                QuantityOnHand = @event.QuantityOnHand,
                QuantityReserved = @event.QuantityReserved,
                UnitCost = @event.UnitCost
            },
            cancellationToken);
    }
}