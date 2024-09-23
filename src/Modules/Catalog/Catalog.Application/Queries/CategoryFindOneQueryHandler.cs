// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CategoryFindOneQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Category> repository)
    : QueryHandlerBase<CategoryFindOneQuery, Result<Category>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Category>>> Process(
        CategoryFindOneQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(
                    CategoryId.Create(query.CategoryId),
                    cancellationToken: cancellationToken)
                .AnyContext());
    }
}