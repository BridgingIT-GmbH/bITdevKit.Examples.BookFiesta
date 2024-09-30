// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockSnapshotFindOneQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<StockSnapshot> stocksnapshotRepository,
    IGenericRepository<Stock> stockRepository)
    : QueryHandlerBase<StockSnapshotFindOneQuery, Result<StockSnapshot>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<StockSnapshot>>> Process(
        StockSnapshotFindOneQuery query,
        CancellationToken cancellationToken)
    {
        var stockResult = await stockRepository.FindOneResultAsync(
            StockId.Create(query.StockId),
            cancellationToken: cancellationToken);

        if (stockResult.IsFailure)
        {
            return QueryResponse.For<StockSnapshot>(stockResult);
        }

        return QueryResponse.For(
            await stocksnapshotRepository.FindOneResultAsync(
                    StockSnapshotId.Create(query.StockSnapshotId),
                    cancellationToken: cancellationToken)
                .AnyContext());
    }
}