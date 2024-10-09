// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

using BridgingIT.DevKit.Domain;

public class StockFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Stock> repository)
    : QueryHandlerBase<StockFindAllQuery, Result<IEnumerable<Stock>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Stock>>>> Process(
        StockFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                    [new Specification<Stock>(e => e.TenantId == tenantId)],
                    new FindOptions<Stock> { Order = new OrderOption<Stock>(e => e.Sku) },
                    cancellationToken)
                .AnyContext());
    }
}