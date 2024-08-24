// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Application;

using System.Threading;
using System.Threading.Tasks;
using BridgingIT.DevKit.Application.Commands;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

public class TenantCreateCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Tenant> tenantRepository,
    IGenericRepository<Company> companyRepository)
        : CommandHandlerBase<TenantCreateCommand, Result<Tenant>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Tenant>>> Process(
        TenantCreateCommand command, CancellationToken cancellationToken)
    {
        var companyResult = await companyRepository.FindOneResultAsync(
            CompanyId.Create(command.Model.CompanyId), cancellationToken: cancellationToken);

        if (companyResult.IsFailure)
        {
            return CommandResponse.For<Tenant>(companyResult);
        }

        var tenant = Tenant.Create(
            companyResult.Value,
            command.Model.Name,
            EmailAddress.Create(command.Model.ContactEmail));

        await DomainRules.ApplyAsync(
        [
            TenantRules.NameMustBeUnique(tenantRepository, tenant),
        ]);

        await tenantRepository.InsertAsync(tenant, cancellationToken).AnyContext();

        return CommandResponse.Success(tenant);
    }
}