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

public class CompanyCreateCommandHandler(
    ILoggerFactory loggerFactory,
    IMapper mapper,
    IGenericRepository<Company> repository)
        : CommandHandlerBase<CompanyCreateCommand, Result<Company>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Company>>> Process(
        CompanyCreateCommand command, CancellationToken cancellationToken)
    {
        //var company = CompanyModelMapper.Map(command.Model);
        var company = mapper.Map<CompanyModel, Company>(command.Model);

        await DomainRules.ApplyAsync([
            CompanyRules.NameMustBeUnique(repository, company),
        ], cancellationToken);

        await repository.InsertAsync(company, cancellationToken).AnyContext();

        return CommandResponse.Success(company);
    }
}
