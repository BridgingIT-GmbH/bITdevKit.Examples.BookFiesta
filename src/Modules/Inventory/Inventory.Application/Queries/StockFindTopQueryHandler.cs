// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockFindTopQueryHandler(
    ILoggerFactory loggerFactory,
    IInventoryQueryService queryService)
    : QueryHandlerBase<StockFindTopQuery, Result<IEnumerable<Stock>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Stock>>>> Process(
        StockFindTopQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await queryService.StockFindTopAsync(query.Start, query.End, query.Limit));
    }
}