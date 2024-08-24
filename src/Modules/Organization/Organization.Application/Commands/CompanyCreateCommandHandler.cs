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

public class CompanyCreateCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Company> repository)
        : CommandHandlerBase<CompanyCreateCommand, Result<Company>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Company>>> Process(
        CompanyCreateCommand command, CancellationToken cancellationToken)
    {
        var company = Company.Create(
            command.Model.Name,
            command.Model.RegistrationNumber,
            command.Model.ContactEmail,
            Address.Create(
                command.Model.Address.Name,
                command.Model.Address.Line1,
                command.Model.Address.Line2,
                command.Model.Address.PostalCode,
                command.Model.Address.City,
                command.Model.Address.Country));

        await DomainRules.ApplyAsync(
        [
            CompanyRules.NameMustBeUnique(repository, company),
        ]);

        await repository.InsertAsync(company, cancellationToken).AnyContext();

        return CommandResponse.Success(company);
    }
}