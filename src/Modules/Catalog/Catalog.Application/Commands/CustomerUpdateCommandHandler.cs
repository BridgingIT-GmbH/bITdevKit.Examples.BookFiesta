// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CustomerUpdateCommandHandler(ILoggerFactory loggerFactory, IGenericRepository<Customer> repository)
    : CommandHandlerBase<CustomerUpdateCommand, Result<Customer>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Customer>>> Process(
        CustomerUpdateCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId =
            TenantId.Create(command.TenantId); // TODO: use in findone query or check later > notfoundexception
        var customerResult = await repository.FindOneResultAsync(
            CustomerId.Create(command.Model.Id),
            cancellationToken: cancellationToken);

        if (customerResult.IsFailure)
        {
            return CommandResponse.For(customerResult);
        }

        DomainRules.Apply([]);

        customerResult.Value.SetName(
            PersonFormalName.Create(
                command.Model.PersonName.Parts,
                command.Model.PersonName.Title,
                command.Model.PersonName.Suffix));
        customerResult.Value.SetAddress(
            Address.Create(
                command.Model.Address.Name,
                command.Model.Address.Line1,
                command.Model.Address.Line2,
                command.Model.Address.PostalCode,
                command.Model.Address.City,
                command.Model.Address.Country));

        await repository.UpsertAsync(customerResult.Value, cancellationToken).AnyContext();

        return CommandResponse.Success(customerResult.Value);
    }
}