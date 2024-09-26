// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerDeleteCommandHandler(ILoggerFactory loggerFactory, IGenericRepository<Customer> repository)
    : CommandHandlerBase<CustomerDeleteCommand, Result<Customer>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Customer>>> Process(
        CustomerDeleteCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId =
            TenantId.Create(command.TenantId); // TODO: use in findone query or check later > notfoundexception
        var customerResult = await repository.FindOneResultAsync(
            CustomerId.Create(command.Id),
            cancellationToken: cancellationToken);

        if (customerResult.IsFailure)
        {
            return CommandResponse.For(customerResult);
        }

        await DomainRules.ApplyAsync([], cancellationToken);

        await repository.DeleteAsync(customerResult.Value, cancellationToken).AnyContext();

        return CommandResponse.Success(customerResult.Value);
    }
}