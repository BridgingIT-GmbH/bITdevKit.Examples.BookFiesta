// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Organization.Application;

using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookStore.Organization.Domain;
using Microsoft.Extensions.Logging;

public class CompanyFindOneQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Company> repository)
        : QueryHandlerBase<CompanyFindOneQuery, Result<Company>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Company>>> Process(
        CompanyFindOneQuery query, CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(
                CompanyId.Create(query.CompanyId),
                cancellationToken: cancellationToken).AnyContext());
    }
}
