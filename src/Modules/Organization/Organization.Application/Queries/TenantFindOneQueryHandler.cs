// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Organization.Application;

using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookStore.Organization.Domain;
using BridgingIT.DevKit.Examples.BookStore.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

public class TenantFindOneQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Tenant> repository)
        : QueryHandlerBase<TenantFindOneQuery, Result<Tenant>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Tenant>>> Process(
        TenantFindOneQuery query, CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(
                TenantId.Create(query.TenantId),
                cancellationToken: cancellationToken).AnyContext());
    }
}
