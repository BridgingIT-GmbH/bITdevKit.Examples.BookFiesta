﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using BridgingIT.DevKit.Domain.Specifications;

public class CustomerFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Customer> repository)
    : QueryHandlerBase<CustomerFindAllQuery, Result<IEnumerable<Customer>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Customer>>>> Process(
        CustomerFindAllQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindAllResultAsync(
                [new Specification<Customer>(e => e.TenantId == query.TenantId)],
                new FindOptions<Customer> { Order = new OrderOption<Customer>(e => e.Email.Value) },
                cancellationToken: cancellationToken).AnyContext());
    }
}