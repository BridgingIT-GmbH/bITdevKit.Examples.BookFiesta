// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.IntegrationTests.Infrastructure;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Infrastructure;
using BridgingIT.DevKit.Examples.BookFiesta.Shared.IntegrationTests.Infrastructure;
using BridgingIT.DevKit.Infrastructure.EntityFramework.Repositories;

[IntegrationTest("Infrastructure")]
[Collection(nameof(TestEnvironmentCollection))] // https://xunit.net/docs/shared-context#collection-fixture
public class OrganizationCompanyRepositoryTests
{
    private readonly TestEnvironmentFixture fixture;
    private readonly OrganizationDbContext context;

    public OrganizationCompanyRepositoryTests(ITestOutputHelper output, TestEnvironmentFixture fixture)
    {
        this.fixture = fixture.WithOutput(output);
        this.context = this.fixture.OrganizationDbContext; //this.fixture.CreateSqlServerDbContext();
    }

    [Fact]
    public async Task FindOneTest()
    {
        // Arrange
        var entities = OrganizationSeedEntities.Create(); // entities are already seeded in the db (fixture)
        var company = entities.Companies[0];
        var sut = new EntityFrameworkGenericRepository<Company>(r => r.DbContext(this.context));

        // Act
        var result = await sut.FindOneAsync(company.Id);

        // Assert
        this.fixture.Output.WriteLine($"Entity: {result.DumpText()}");
        result.ShouldNotBeNull();
        result.Id.Value.ShouldBe(company.Id.Value);
    }

    // private async Task<Company> InsertCompanyAsync()
    // {
    //     var ticks = DateTime.UtcNow.Ticks;
    //
    //     var company = OrganizationSeedEntities.Companies.Create(ticks).First();
    //     var companyRepository = new EntityFrameworkGenericRepository<Company>(r => r.DbContext(this.context));
    //
    //     await companyRepository.InsertAsync(company);
    //
    //     return company;
    // }
}