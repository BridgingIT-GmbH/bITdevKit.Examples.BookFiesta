// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Application;

using BridgingIT.DevKit.Application.Commands;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using Microsoft.Extensions.Logging;

public class CustomerUpdateCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Customer> repository)
        : CommandHandlerBase<CustomerUpdateCommand, Result<Customer>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Customer>>> Process(
        CustomerUpdateCommand command, CancellationToken cancellationToken)
    {
        var customerResult = await repository.FindOneResultAsync(
            AuthorId.Create(command.Id), cancellationToken: cancellationToken);
        var customer = customerResult.Value;
        if (customerResult.IsFailure)
        {
            return CommandResponse.Create(customerResult);
        }

        Check.Throw([]);

        customer.ChangeName(command.FirstName, command.LastName);
        customer.ChangeAddress(command.AddressLine1, command.AddressLine2, command.AddressPostalCode, command.AddressCity, command.AddressCountry);

        await repository.UpsertAsync(customer, cancellationToken).AnyContext();

        return CommandResponse.Success(customer);
    }
}