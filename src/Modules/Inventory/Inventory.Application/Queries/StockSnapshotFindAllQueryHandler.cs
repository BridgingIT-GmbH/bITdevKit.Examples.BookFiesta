// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

using BridgingIT.DevKit.Domain.Specifications;

public class StockSnapshotFindAllQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<StockSnapshot> stocksnapshotRepository,
    IGenericRepository<Stock> stockRepository)
    : QueryHandlerBase<StockSnapshotFindAllQuery, Result<IEnumerable<StockSnapshot>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<StockSnapshot>>>> Process(
        StockSnapshotFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);
        var stockResult = await stockRepository.FindOneResultAsync(
            StockId.Create(query.StockId),
            cancellationToken: cancellationToken);

        if (stockResult.IsFailure)
        {
            return QueryResponse.For<IEnumerable<StockSnapshot>>(stockResult);
        }

        return QueryResponse.For(
            await stocksnapshotRepository.FindAllResultAsync(
                    [new Specification<StockSnapshot>(e => e.TenantId == tenantId && e.StockId == query.StockId)],
                    new FindOptions<StockSnapshot> { Order = new OrderOption<StockSnapshot>(e => e.Timestamp) },
                    cancellationToken)
                .AnyContext());
    }
}