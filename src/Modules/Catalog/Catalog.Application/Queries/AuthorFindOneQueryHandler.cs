// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class AuthorFindOneQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Author> repository)
    : QueryHandlerBase<AuthorFindOneQuery, Result<Author>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Author>>> Process(
        AuthorFindOneQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(AuthorId.Create(query.AuthorId), cancellationToken: cancellationToken)
                .AnyContext());
    }
}