﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using Common;
using DevKit.Application.Queries;
using DevKit.Domain.Repositories;
using Domain;
using Microsoft.Extensions.Logging;

public class CustomerFindOneQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Customer> repository)
    : QueryHandlerBase<CustomerFindOneQuery, Result<Customer>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Customer>>> Process(CustomerFindOneQuery query, CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(CustomerId.Create(query.CustomerId), cancellationToken: cancellationToken)
                .AnyContext());
    }
}