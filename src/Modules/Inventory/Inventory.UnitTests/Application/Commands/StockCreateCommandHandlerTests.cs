// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Inventory.UnitTests.Application;

using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[UnitTest("Inventory:Application")]
public class StockCreateCommandHandlerTests
{
    [Fact]
    public async Task Process_ValidRequest_SuccessResult()
    {
        // Arrange
        var repository = Substitute.For<IGenericRepository<Stock>>();
        TenantId[] tenantIds =
        [
            TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")
        ];
        var stock = InventorySeedEntities.Stocks.Create(tenantIds, DateTime.UtcNow.Ticks)[0];
        var model = new StockModel
        {
            TenantId = stock.TenantId,
            Sku = stock.Sku,
            QuantityOnHand = stock.QuantityOnHand,
            QuantityReserved = stock.QuantityReserved,
            ReorderThreshold = stock.ReorderThreshold,
            ReorderQuantity = stock.ReorderThreshold,
            StorageLocation = stock.StorageLocation,
            UnitCost = stock.UnitCost,
            LastRestockedAt = stock.LastRestockedAt
        };
        var command = new StockCreateCommand(tenantIds[0], model);
        var sut = new StockCreateCommandHandler(Substitute.For<ILoggerFactory>(), repository);

        // Act
        var response = await sut.Process(command, CancellationToken.None);

        // Assert
        response?.Result.ShouldNotBeNull();
        response?.Result.Value.Sku.Value.ShouldBe(command.Model.Sku);
        await repository.Received(1).InsertAsync(Arg.Any<Stock>(), CancellationToken.None);
    }
}