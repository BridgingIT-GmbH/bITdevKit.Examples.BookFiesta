// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class PublisherFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Publisher> repository)
    : QueryHandlerBase<PublisherFindAllQuery, Result<IEnumerable<Publisher>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Publisher>>>> Process(
        PublisherFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                    [new Specification<Publisher>(e => e.TenantId == tenantId)],
                    new FindOptions<Publisher> { Order = new OrderOption<Publisher>(e => e.Name) },
                    cancellationToken)
                .AnyContext());
    }
}