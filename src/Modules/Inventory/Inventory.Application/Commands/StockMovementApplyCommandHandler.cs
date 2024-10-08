// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockMovementApplyCommandHandler(ILoggerFactory loggerFactory, IGenericRepository<Stock> repository)
    : CommandHandlerBase<StockMovementApplyCommand, Result<Stock>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Stock>>> Process(
        StockMovementApplyCommand command,
        CancellationToken cancellationToken)
    {
        var stockResult = await repository.FindOneResultAsync(
            StockId.Create(command.StockId),
            cancellationToken: cancellationToken);

        if (stockResult.IsFailure)
        {
            return CommandResponse.For(stockResult);
        }

        if (command.Model.Type == StockMovementType.Addition.Id)
        {
            stockResult.Value.AddStock(command.Model.Quantity);
            // -> register StockUpdatedDomainEvent -> Handler -> publish StockUpdatedMessage
        }
        else if (command.Model.Type == StockMovementType.Removal.Id)
        {
            stockResult.Value.RemoveStock(command.Model.Quantity);
            // -> register StockUpdatedDomainEvent -> Handler -> publish StockUpdatedMessage
        }
        else
        {
            throw new DomainRuleException("Stock movement type not supported");
        }

        await DomainRules.ApplyAsync([], cancellationToken);

        await repository.UpdateAsync(stockResult.Value, cancellationToken).AnyContext(); // -> dispatch DomainEvents

        return CommandResponse.Success(stockResult.Value);
    }
}