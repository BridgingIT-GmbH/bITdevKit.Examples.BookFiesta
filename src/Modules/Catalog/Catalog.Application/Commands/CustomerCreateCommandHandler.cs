// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Application;

using System.Threading;
using System.Threading.Tasks;
using BridgingIT.DevKit.Application.Commands;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

public class CustomerCreateCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Customer> repository)
        : CommandHandlerBase<CustomerCreateCommand, Result<Customer>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Customer>>> Process(
        CustomerCreateCommand command, CancellationToken cancellationToken)
    {
        var customer = Customer.Create(
            TenantId.Create(command.TenantId),
            command.FirstName,
            command.LastName,
            EmailAddress.Create(command.Email),
            Address.Create(command.AddressName, command.AddressLine1, command.AddressLine2,
                           command.AddressPostalCode, command.AddressCity, command.AddressCountry)
        );

        await DomainRules.ApplyAsync(
        [
            CustomerRules.EmailMustBeUnique(repository, customer),
        ]);

        await repository.InsertAsync(customer, cancellationToken).AnyContext();

        return CommandResponse.Success(customer);
    }
}