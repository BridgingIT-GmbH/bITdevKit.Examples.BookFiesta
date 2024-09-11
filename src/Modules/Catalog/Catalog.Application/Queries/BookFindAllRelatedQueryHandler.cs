// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using System.Collections.Generic;
using System.Threading.Tasks;
using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using Microsoft.Extensions.Logging;

public class BookFindAllRelatedQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Book> repository,
    ICatalogQueryService recommendationService)
        : QueryHandlerBase<BookFindAllRelatedQuery, Result<IEnumerable<Book>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Book>>>> Process(
        BookFindAllRelatedQuery query, CancellationToken cancellationToken)
    {
        var bookId = BookId.Create(query.BookId);
        var result = await repository.FindOneResultAsync(
            bookId, cancellationToken: cancellationToken).AnyContext() ?? throw new EntityNotFoundException();

        return result.IsSuccess
            ? QueryResponse.For(
                await recommendationService.BookFindAllRelatedAsync(result.Value))
            : QueryResponse.For<IEnumerable<Book>>(result);
    }
}