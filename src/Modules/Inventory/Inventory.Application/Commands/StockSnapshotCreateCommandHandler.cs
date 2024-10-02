// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockSnapshotCreateCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Stock> stockRepository,
    IGenericRepository<StockSnapshot> stockSnapshotRepository)
    : CommandHandlerBase<StockSnapshotCreateCommand, Result<StockSnapshot>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<StockSnapshot>>> Process(
        StockSnapshotCreateCommand command,
        CancellationToken cancellationToken)
    {
        var stockResult = await stockRepository.FindOneResultAsync(
            StockId.Create(command.StockId),
            cancellationToken: cancellationToken);

        if (stockResult.IsFailure)
        {
            return CommandResponse.For<StockSnapshot>(stockResult);
        }

        await DomainRules.ApplyAsync([], cancellationToken);

        var stockSnapshot = StockSnapshot.Create(
            stockResult.Value.TenantId,
            stockResult.Value.Id,
            stockResult.Value.Sku,
            stockResult.Value.QuantityOnHand,
            stockResult.Value.QuantityReserved,
            stockResult.Value.UnitCost,
            stockResult.Value.Location,
            DateTimeOffset.UtcNow);

        await stockSnapshotRepository.InsertAsync(stockSnapshot, cancellationToken).AnyContext();

        return CommandResponse.Success(stockSnapshot);
    }
}