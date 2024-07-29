// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Application;

using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Domain.Specifications;
using BridgingIT.DevKit.Examples.BookFiesta.Catalog.Domain;
using Microsoft.Extensions.Logging;

public class BookFindAllForCategoryQueryHandler(
    ILoggerFactory loggerFactory, IGenericRepository<Book> repository)
        : QueryHandlerBase<BookFindAllForCategoryQuery, Result<IEnumerable<Book>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Book>>>> Process(
        BookFindAllForCategoryQuery query, CancellationToken cancellationToken)
    {
        var categoryId = CategoryId.Create(query.CategoryId);

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                specification: new Specification<Book>(e => e.Categories.Any(c => c.Id == categoryId)),
                cancellationToken: cancellationToken).AnyContext());
    }
}