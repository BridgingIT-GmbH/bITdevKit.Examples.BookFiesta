// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using Money = BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain.Money;

public class StockCreateCommandHandler(ILoggerFactory loggerFactory, IGenericRepository<Stock> repository)
    : CommandHandlerBase<StockCreateCommand, Result<Stock>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Stock>>> Process(
        StockCreateCommand command,
        CancellationToken cancellationToken)
    {
        var stock = Stock.Create(
            TenantId.Create(command.TenantId),
            ProductSku.Create(command.Model.Sku),
            command.Model.QuantityOnHand,
            command.Model.ReorderThreshold,
            command.Model.ReorderQuantity,
            Money.Create(command.Model.UnitCost),
            StorageLocation.Create("A", "1", "1"));
        // -> register StockCreatedDomainEvent -> Handler -> publish StockCreatedMessage

        await DomainRules.ApplyAsync(
            [
                StockRules.SkuMustBeUnique(repository, stock)
            ],
            cancellationToken);

        await repository.InsertAsync(stock, cancellationToken).AnyContext(); // -> dispatch DomainEvents

        return CommandResponse.Success(stock);
    }
}