// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Application;

using BridgingIT.DevKit.Application.Commands;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;
using Microsoft.Extensions.Logging;

public class CompanyDeleteCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Company> companyRepository,
    IGenericRepository<Tenant> tenantRepository)
        : CommandHandlerBase<CompanyDeleteCommand, Result<Company>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Company>>> Process(
        CompanyDeleteCommand command, CancellationToken cancellationToken)
    {
        var companyResult = await companyRepository.FindOneResultAsync(
            CompanyId.Create(command.Id), cancellationToken: cancellationToken);

        if (companyResult.IsFailure)
        {
            return CommandResponse.For(companyResult);
        }

        DomainRules.Apply([
            CompanyRules.MustHaveNoTenants(tenantRepository, companyResult.Value)
        ]);

        await companyRepository.DeleteAsync(companyResult.Value, cancellationToken).AnyContext();

        return CommandResponse.Success(companyResult.Value);
    }
}