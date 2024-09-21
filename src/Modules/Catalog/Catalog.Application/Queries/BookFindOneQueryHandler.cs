// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using Common;
using DevKit.Application.Queries;
using DevKit.Domain.Repositories;
using Domain;
using Microsoft.Extensions.Logging;
using Organization.Application;

public class BookFindOneQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Book> repository)
    : QueryHandlerBase<BookFindOneQuery, Result<Book>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Book>>> Process(BookFindOneQuery query, CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(BookId.Create(query.BookId), cancellationToken: cancellationToken)
                .AnyContext());
    }
}