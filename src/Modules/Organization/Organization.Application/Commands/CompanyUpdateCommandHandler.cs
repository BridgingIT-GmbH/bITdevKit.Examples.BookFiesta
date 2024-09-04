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

public class CompanyUpdateCommandHandler(
    ILoggerFactory loggerFactory,
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

        await DomainRules.ApplyAsync([
            CompanyRules.NameMustBeUnique(repository, companyResult.Value),
        ], cancellationToken);

        companyResult.Value.SetName(command.Model.Name);
        companyResult.Value.SetRegistrationNumber(command.Model.RegistrationNumber);
        companyResult.Value.SetContactEmail(command.Model.ContactEmail);
        companyResult.Value.SetAddress(
            Address.Create(
                command.Model.Address.Name,
                command.Model.Address.Line1,
                command.Model.Address.Line2,
                command.Model.Address.PostalCode,
                command.Model.Address.City,
                command.Model.Address.Country));

        await repository.UpdateAsync(companyResult.Value, cancellationToken).AnyContext();

        return CommandResponse.Success(companyResult.Value);
    }
}