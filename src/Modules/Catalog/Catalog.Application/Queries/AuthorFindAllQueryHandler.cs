// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using BridgingIT.DevKit.Domain;

public class AuthorFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Author> repository)
    : QueryHandlerBase<AuthorFindAllQuery, Result<IEnumerable<Author>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Author>>>> Process(
        AuthorFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                [new Specification<Author>(e => e.TenantId == tenantId)],
                cancellationToken: cancellationToken).AnyContext());
    }
}