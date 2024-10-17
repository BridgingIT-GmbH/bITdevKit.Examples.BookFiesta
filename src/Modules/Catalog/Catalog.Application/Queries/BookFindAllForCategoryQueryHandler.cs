// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookFindAllForCategoryQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Book> repository)
    : QueryHandlerBase<BookFindAllForCategoryQuery, Result<IEnumerable<Book>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Book>>>> Process(
        BookFindAllForCategoryQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);
        var categoryId = CategoryId.Create(query.CategoryId);

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                    new Specification<Book>(e => e.Categories.Any(c => c.TenantId == tenantId && c.Id == categoryId)),
                    cancellationToken: cancellationToken)
                .AnyContext());
    }
}