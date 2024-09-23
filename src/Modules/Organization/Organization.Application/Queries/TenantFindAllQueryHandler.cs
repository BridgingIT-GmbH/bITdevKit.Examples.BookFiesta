// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using DevKit.Domain.Specifications;

public class TenantFindAllQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Tenant> repository)
    : QueryHandlerBase<TenantFindAllQuery, Result<IEnumerable<Tenant>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Tenant>>>> Process(
        TenantFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var specifications = new List<ISpecification<Tenant>>();

        if (!query.CompanyId.IsNullOrEmpty())
        {
            specifications.Add(TenantSpecifications.ForCompany(CompanyId.Create(query.CompanyId)));
        }

        return QueryResponse.For(
            await repository.FindAllResultAsync(
                    specifications,
                    new FindOptions<Tenant> { Order = new OrderOption<Tenant>(e => e.Name) },
                    cancellationToken)
                .AnyContext());
    }
}