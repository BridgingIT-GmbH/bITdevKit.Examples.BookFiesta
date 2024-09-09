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

public class CompanyUpdateCommandHandler(
    ILoggerFactory loggerFactory,
    IMapper mapper,
    IGenericRepository<Company> repository)
        : CommandHandlerBase<CompanyUpdateCommand, Result<Company>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Company>>> Process(
        CompanyUpdateCommand command, CancellationToken cancellationToken)
    {
        var companyResult = await repository.FindOneResultAsync(
            CompanyId.Create(command.Model.Id), cancellationToken: cancellationToken);

        if (companyResult.IsFailure)
        {
            return CommandResponse.For(companyResult);
        }

        //var company = CompanyModelMapper.Map(command.Model, companyResult.Value);
        var company = mapper.Map<CompanyModel, Company>(command.Model, companyResult.Value);

        await DomainRules.ApplyAsync([
            CompanyRules.NameMustBeUnique(repository, company),
        ], cancellationToken);

        await repository.UpdateAsync(company, cancellationToken).AnyContext();

        return CommandResponse.Success(company);
    }
}