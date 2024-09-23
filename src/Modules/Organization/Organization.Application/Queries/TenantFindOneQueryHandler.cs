// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantFindOneQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Tenant> repository)
    : QueryHandlerBase<TenantFindOneQuery, Result<Tenant>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Tenant>>> Process(
        TenantFindOneQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(TenantId.Create(query.TenantId), cancellationToken: cancellationToken)
                .AnyContext());
    }
}