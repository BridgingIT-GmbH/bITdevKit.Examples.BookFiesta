// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using System.Collections.Generic;
using System.Threading.Tasks;
using BridgingIT.DevKit.Application.Queries;
using Common;
using BridgingIT.DevKit.Domain.Repositories;
using Domain;
using Microsoft.Extensions.Logging;

public class CompanyFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Company> repository)
    : QueryHandlerBase<CompanyFindAllQuery, Result<IEnumerable<Company>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Company>>>> Process(CompanyFindAllQuery query, CancellationToken cancellationToken)
    {
        return QueryResponse.For(await repository.FindAllResultAsync(new FindOptions<Company>() { Order = new OrderOption<Company>(e => e.Name) }, cancellationToken)
            .AnyContext());
    }
}