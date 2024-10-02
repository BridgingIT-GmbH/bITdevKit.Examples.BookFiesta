// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.IntegrationTests.Infrastructure;

using System.Linq.Dynamic.Core;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Infrastructure;
using BridgingIT.DevKit.Examples.BookFiesta.Shared.IntegrationTests.Infrastructure;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;

[IntegrationTest("Infrastructure")]
[Collection(nameof(TestEnvironmentCollection))] // https://xunit.net/docs/shared-context#collection-fixture
public class InventoryStockRepositoryTests
{
    private readonly TestEnvironmentFixture fixture;
    private readonly InventoryDbContext context;

    public InventoryStockRepositoryTests(ITestOutputHelper output, TestEnvironmentFixture fixture)
    {
        this.fixture = fixture.WithOutput(output);
        this.context = this.fixture.InventoryDbContext; //this.fixture.CreateSqlServerDbContext();
    }

    [Fact]
    public async Task FindOneTest()
    {
        // Arrange
        TenantId[] tenants = [TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")];
        var entities = InventorySeedEntities.Create(tenants); // entities are already seeded in the db (fixture)
        var stock = entities.Stocks[0];
        var sut = new EntityFrameworkGenericRepository<Stock>(r => r.DbContext(this.context));

        // Act
        var result = await sut.FindOneAsync(stock.Id);

        // Assert
        this.fixture.Output.WriteLine($"Entity: {result.DumpText()}");
        result.ShouldNotBeNull();
        result.Id.Value.ShouldBe(stock.Id.Value);
    }

    // private async Task<Stock> InsertEntityAsync()
    // {
    //     var ticks = DateTime.UtcNow.Ticks;
    //     TenantId[] tenants = [TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")];
    //     var entity = InventorySeedEntities.Stocks.Create(tenants, ticks).First();
    //     var sut = new EntityFrameworkGenericRepository<Stock>(r => r.DbContext(this.context));
    //
    //     return await sut.InsertAsync(entity);
    // }
}