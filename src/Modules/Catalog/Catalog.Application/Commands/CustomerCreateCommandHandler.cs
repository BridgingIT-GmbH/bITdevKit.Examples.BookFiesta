// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Application;

using System.Threading;
using System.Threading.Tasks;
using BridgingIT.DevKit.Application.Commands;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookStore.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

public class CustomerCreateCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Customer> repository)
        : CommandHandlerBase<CustomerCreateCommand, Result<Customer>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Customer>>> Process(
        CustomerCreateCommand command, CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(command.TenantId);
        var email = EmailAddress.Create(command.Email);
        var address = Address.Create(command.AddressName, command.AddressLine1, command.AddressLine2, command.AddressPostalCode, command.AddressCity, command.AddressCountry);
        var customer = Customer.Create(tenantId, command.FirstName, command.LastName, email, address);

        Check.Throw(
        [
            CustomerRules.EmailMustBeUnique(repository, customer),
        ]);

        await repository.InsertAsync(customer, cancellationToken).AnyContext();

        return CommandResponse.Success(customer);
    }
}