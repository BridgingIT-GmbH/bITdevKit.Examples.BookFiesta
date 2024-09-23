// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

public class TenantUpdateCommandHandler(
    ILoggerFactory loggerFactory,
    IMapper mapper,
    IGenericRepository<Tenant> tenantRepository,
    IGenericRepository<Company> companyRepository)
    : CommandHandlerBase<TenantUpdateCommand, Result<Tenant>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Tenant>>> Process(
        TenantUpdateCommand command,
        CancellationToken cancellationToken)
    {
        var companyResult = await companyRepository.FindOneResultAsync(
            CompanyId.Create(command.Model.CompanyId),
            cancellationToken: cancellationToken);

        if (companyResult.IsFailure)
        {
            return CommandResponse.For<Tenant>(companyResult);
        }

        var tenantResult = await tenantRepository.FindOneResultAsync(
            TenantId.Create(command.Model.Id),
            cancellationToken: cancellationToken);

        if (tenantResult.IsFailure)
        {
            return CommandResponse.For(tenantResult);
        }

        //var tenant = TenantModelMapper.Map(command.Model, tenantResult.Value);
        var tenant = mapper.Map(command.Model, tenantResult.Value);

        await DomainRules.ApplyAsync([TenantRules.NameMustBeUnique(tenantRepository, tenant)], cancellationToken);

        await tenantRepository.UpdateAsync(tenant, cancellationToken).AnyContext();

        return CommandResponse.Success(tenant);
    }
}