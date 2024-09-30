// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockFindOneQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Stock> repository)
    : QueryHandlerBase<StockFindOneQuery, Result<Stock>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Stock>>> Process(
        StockFindOneQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(
                    StockId.Create(query.StockId),
                    cancellationToken: cancellationToken)
                .AnyContext());
    }
}