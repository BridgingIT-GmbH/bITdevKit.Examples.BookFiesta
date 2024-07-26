// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Application;

using System.Collections.Generic;
using System.Threading.Tasks;
using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using Microsoft.Extensions.Logging;

public class BookFindAllQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Book> repository)
        : QueryHandlerBase<BookFindAllQuery, Result<IEnumerable<Book>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Book>>>> Process(
        BookFindAllQuery query, CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindAllResultAsync(
                new FindOptions<Book>() { Order = new OrderOption<Book>(e => e.Title) }, cancellationToken: cancellationToken).AnyContext());
    }
}