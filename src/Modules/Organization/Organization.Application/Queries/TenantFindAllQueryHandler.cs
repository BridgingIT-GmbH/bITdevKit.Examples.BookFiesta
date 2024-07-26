// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Organization.Application;

using System.Collections.Generic;
using System.Threading.Tasks;
using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookStore.Organization.Domain;
using Microsoft.Extensions.Logging;

public class TenantFindAllQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Tenant> repository)
        : QueryHandlerBase<TenantFindAllQuery, Result<IEnumerable<Tenant>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Tenant>>>> Process(
        TenantFindAllQuery query, CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindAllResultAsync(
                new FindOptions<Tenant>() { Order = new OrderOption<Tenant>(e => e.Name) }, cancellationToken: cancellationToken).AnyContext());
    }
}
