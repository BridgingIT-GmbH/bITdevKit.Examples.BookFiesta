// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.UnitTests.Application;

using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[UnitTest("Inventory:Application")]
public class StockFindAllQueryHandlerTests
{
    [Fact]
    public async Task Process_ValidQuery_ReturnsSuccessResultWithStocks()
    {
        // Arrange
        TenantId[] tenantIds =
        [
            TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")
        ];
        var expectedStocks = new List<Stock>
        {
            Stock.Create(tenantIds[0], ProductSku.Create("1111111111"), 100, 0, 0, Money.Create(10m), StorageLocation.Create("A", "1", "1") ),
            Stock.Create(tenantIds[0], ProductSku.Create("2222222222"), 100, 0, 0, Money.Create(10m), StorageLocation.Create("A", "1", "1") )
        };

        var repository = Substitute.For<IGenericRepository<Stock>>();
        repository.FindAllAsync(
                Arg.Any<IEnumerable<ISpecification<Stock>>>(),
                Arg.Any<FindOptions<Stock>>(),
                cancellationToken: CancellationToken.None)
            .Returns(expectedStocks.AsEnumerable());

        var query = new StockFindAllQuery(tenantIds[0]);
        var sut = new StockFindAllQueryHandler(Substitute.For<ILoggerFactory>(), repository);

        // Act
        var response = await sut.Process(query, CancellationToken.None);

        // Assert
        response?.Result.ShouldNotBeNull().ShouldBeSuccess();
        response?.Result.Value.ShouldNotBeNull();
        response?.Result.Value.Count().ShouldBe(expectedStocks.Count);

        await repository.Received(1).FindAllAsync(
            Arg.Any<IEnumerable<ISpecification<Stock>>>(),
            Arg.Any<FindOptions<Stock>>(),
            cancellationToken: CancellationToken.None);
    }
}