// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application.Messages;

using BridgingIT.DevKit.Application.Messaging;
using BridgingIT.DevKit.Domain.Specifications;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockUpdatedMessageHandler(ILoggerFactory loggerFactory, IGenericRepository<Book> repository)
    : MessageHandlerBase<StockUpdatedMessage>(loggerFactory)
{
    public override async Task Handle(StockUpdatedMessage message, CancellationToken cancellationToken)
    {
        var book = (await repository.FindAllResultAsync(
            new Specification<Book>(e => e.TenantId == message.TenantId && e.Sku == message.Sku),
            cancellationToken: cancellationToken)).Value.FirstOrDefault();

        if (book == null)
        {
            // TODO: log book not found by sku
            return;
        }

        book.SetStock(message.QuantityOnHand, message.QuantityReserved);
        await repository.UpdateAsync(book, cancellationToken);
    }
}