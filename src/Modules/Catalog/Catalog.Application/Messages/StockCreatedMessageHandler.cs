// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application.Messages;

using BridgingIT.DevKit.Application.Messaging;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockCreatedMessageHandler(ILoggerFactory loggerFactory)
    : MessageHandlerBase<StockCreatedMessage>(loggerFactory)
{
    public override Task Handle(StockCreatedMessage message, CancellationToken cancellationToken)
    {
        // update book by sku > message.QuantityOnHand
        throw new NotImplementedException();
    }
}