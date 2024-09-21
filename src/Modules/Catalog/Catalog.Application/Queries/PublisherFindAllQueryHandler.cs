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

public class PublisherFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Publisher> repository)
    : QueryHandlerBase<PublisherFindAllQuery, Result<IEnumerable<Publisher>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Publisher>>>> Process(PublisherFindAllQuery query, CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindAllResultAsync(new FindOptions<Publisher> { Order = new OrderOption<Publisher>(e => e.Name) }, cancellationToken)
                .AnyContext());
    }
}