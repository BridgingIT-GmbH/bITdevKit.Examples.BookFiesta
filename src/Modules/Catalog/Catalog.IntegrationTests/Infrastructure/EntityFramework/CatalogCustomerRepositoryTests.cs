// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.IntegrationTests.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Infrastructure;
using BridgingIT.DevKit.Examples.BookFiesta.Shared.IntegrationTests.Infrastructure;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
using BridgingIT.DevKit.Infrastructure.EntityFramework.Repositories;

[IntegrationTest("Infrastructure")]
[Collection(nameof(TestEnvironmentCollection))] // https://xunit.net/docs/shared-context#collection-fixture
public class CatalogCustomerRepositoryTests
{
    private readonly TestEnvironmentFixture fixture;
    private readonly CatalogDbContext context;

    public CatalogCustomerRepositoryTests(ITestOutputHelper output, TestEnvironmentFixture fixture)
    {
        this.fixture = fixture.WithOutput(output);
        this.context = this.fixture.CatalogDbContext; //this.fixture.CreateSqlServerDbContext();
    }

    [Fact]
    public async Task FindOneTest()
    {
        // Arrange
        TenantId[] tenants = [TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")];
        var entities = CatalogSeedEntities.Create(tenants); // entities are already seeded in the db (fixture)
        var customer = entities.Customer[0];
        var sut = new EntityFrameworkGenericRepository<Customer>(r => r.DbContext(this.context));

        // Act
        var result = await sut.FindOneAsync(customer.Id);

        // Assert
        this.fixture.Output.WriteLine($"Entity: {result.DumpText()}");
        result.ShouldNotBeNull();
        result.Id.Value.ShouldBe(customer.Id.Value);
    }
}