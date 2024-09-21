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

public class PublisherFindOneQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Publisher> repository)
    : QueryHandlerBase<PublisherFindOneQuery, Result<Publisher>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Publisher>>> Process(PublisherFindOneQuery query, CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(PublisherId.Create(query.PublisherId), cancellationToken: cancellationToken)
                .AnyContext());
    }
}