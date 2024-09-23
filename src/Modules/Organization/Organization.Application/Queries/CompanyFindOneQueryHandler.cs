// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class CompanyFindOneQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Company> repository)
    : QueryHandlerBase<CompanyFindOneQuery, Result<Company>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<Company>>> Process(
        CompanyFindOneQuery query,
        CancellationToken cancellationToken)
    {
        return QueryResponse.For(
            await repository.FindOneResultAsync(CompanyId.Create(query.CompanyId), cancellationToken: cancellationToken)
                .AnyContext());
    }
}