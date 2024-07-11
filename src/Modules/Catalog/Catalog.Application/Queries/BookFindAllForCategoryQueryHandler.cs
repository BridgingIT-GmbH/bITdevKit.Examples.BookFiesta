// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Application;

using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Common;
//using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using Microsoft.Extensions.Logging;

public class BookFindAllForCategoryQueryHandler(
    ILoggerFactory loggerFactory/*, IGenericRepository<Book> repository*/)
        : QueryHandlerBase<BookFindAllForCategoryQuery, Result<IEnumerable<Book>>>(loggerFactory)
{
    public override /*async*/ Task<QueryResponse<Result<IEnumerable<Book>>>> Process(
        BookFindAllForCategoryQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        //return QueryResponse.For(
        //    await repository.FindAllResultAsync(cancellationToken: cancellationToken).AnyContext());
        //    //TODO: add CategoryId specification
    }
}