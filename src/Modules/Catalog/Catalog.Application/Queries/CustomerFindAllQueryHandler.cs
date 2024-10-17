// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Customer> repository)
    : QueryHandlerBase<CustomerFindAllQuery, Result<IEnumerable<Customer>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Customer>>>> Process(
        CustomerFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                [new Specification<Customer>(e => e.TenantId == tenantId)],
                new FindOptions<Customer> { Order = new OrderOption<Customer>(e => e.Email) },
                cancellationToken: cancellationToken).AnyContext());
    }
}