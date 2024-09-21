// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using Common;
using DevKit.Application.Commands;
using DevKit.Domain;
using DevKit.Domain.Repositories;
using Domain;
using Microsoft.Extensions.Logging;
using SharedKernel.Domain;

public class CustomerCreateCommandHandler(ILoggerFactory loggerFactory, IGenericRepository<Customer> repository)
    : CommandHandlerBase<CustomerCreateCommand, Result<Customer>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Customer>>> Process(CustomerCreateCommand command, CancellationToken cancellationToken)
    {
        var customer = Customer.Create(
            TenantId.Create(command.TenantId),
            PersonFormalName.Create(command.Model.PersonName.Parts, command.Model.PersonName.Title, command.Model.PersonName.Suffix),
            EmailAddress.Create(command.Model.Email),
            Address.Create(
                command.Model.Address.Name,
                command.Model.Address.Line1,
                command.Model.Address.Line2,
                command.Model.Address.PostalCode,
                command.Model.Address.City,
                command.Model.Address.Country));

        await DomainRules.ApplyAsync([CustomerRules.EmailMustBeUnique(repository, customer)], cancellationToken);

        await repository.InsertAsync(customer, cancellationToken)
            .AnyContext();

        return CommandResponse.Success(customer);
    }
}