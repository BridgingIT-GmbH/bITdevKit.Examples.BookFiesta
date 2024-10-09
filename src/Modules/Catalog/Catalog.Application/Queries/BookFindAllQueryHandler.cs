// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using BridgingIT.DevKit.Domain;

public class BookFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Book> repository)
    : QueryHandlerBase<BookFindAllQuery, Result<IEnumerable<Book>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Book>>>> Process(
        BookFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                    [new Specification<Book>(e => e.TenantId == tenantId)],
                    new FindOptions<Book> { Order = new OrderOption<Book>(e => e.Title) },
                    cancellationToken)
                .AnyContext());
    }
}