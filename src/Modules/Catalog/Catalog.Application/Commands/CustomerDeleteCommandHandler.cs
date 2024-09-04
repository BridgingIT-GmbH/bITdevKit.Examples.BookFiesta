// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Application;

using BridgingIT.DevKit.Application.Commands;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

public class CustomerDeleteCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Customer> repository)
        : CommandHandlerBase<CustomerDeleteCommand, Result<Customer>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Customer>>> Process(
        CustomerDeleteCommand command, CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(command.TenantId); // TODO: use in findone query or check later > notfoundexception
        var customerResult = await repository.FindOneResultAsync(
            CustomerId.Create(command.Id), cancellationToken: cancellationToken);

        if (customerResult.IsFailure)
        {
            return CommandResponse.For(customerResult);
        }

        DomainRules.Apply([]);

        await repository.DeleteAsync(customerResult.Value, cancellationToken).AnyContext();

        return CommandResponse.Success(customerResult.Value);
    }
}