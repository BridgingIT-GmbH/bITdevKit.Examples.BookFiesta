// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.IntegrationTests.Infrastructure;

using System.Linq.Dynamic.Core;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Infrastructure;
using BridgingIT.DevKit.Infrastructure.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;

[IntegrationTest("Infrastructure")]
[Collection(nameof(TestEnvironmentCollection))] // https://xunit.net/docs/shared-context#collection-fixture
public class OrganizationCompanyRepositoryTests
{
    private readonly TestEnvironmentFixture fixture;
    private readonly OrganizationDbContext context;

    public OrganizationCompanyRepositoryTests(ITestOutputHelper output, TestEnvironmentFixture fixture)
    {
        this.fixture = fixture.WithOutput(output);
        this.context = this.fixture.CreateSqlServerDbContext();
    }

    [Fact]
    public async Task FindOneTest()
    {
        // Arrange
        var entity = await this.InsertEntityAsync();
        var sut = new EntityFrameworkGenericRepository<Company>(r => r.DbContext(this.context));

        // Act
        var result = await sut.FindOneAsync(entity.Id);

        // Assert
        this.fixture.Output.WriteLine($"Entity: {result.DumpText()}");
        this.context.Companies.AsNoTracking().Count().ShouldBeGreaterThanOrEqualTo(1);
        result.ShouldNotBeNull();
        result.Id.Value.ShouldBe(entity.Id.Value);
    }

    private async Task<Company> InsertEntityAsync()
    {
        var ticks = DateTime.UtcNow.Ticks;
        var entity = OrganizationSeedEntities.Companies.Create(ticks).First();
        var sut = new EntityFrameworkGenericRepository<Company>(r => r.DbContext(this.context));

        return await sut.InsertAsync(entity);
    }
}