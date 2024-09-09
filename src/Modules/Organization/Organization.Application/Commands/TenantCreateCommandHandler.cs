// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using System.Threading;
using System.Threading.Tasks;
using BridgingIT.DevKit.Application.Commands;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

public class TenantCreateCommandHandler(
    ILoggerFactory loggerFactory,
    IMapper mapper,
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

        //var tenant = TenantModelMapper.Map(command.Model);
        var tenant = mapper.Map<TenantModel, Tenant>(command.Model);

        await DomainRules.ApplyAsync([
            TenantRules.NameMustBeUnique(tenantRepository, tenant),
        ], cancellationToken);

        await tenantRepository.InsertAsync(tenant, cancellationToken).AnyContext();

        return CommandResponse.Success(tenant);
    }
}
