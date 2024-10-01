// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.UnitTests.Application;

using Bogus;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;

[UnitTest("Organization:Application")]
public class CompanyFindAllQueryHandlerTests
{
    private readonly Faker faker = new();

    [Fact]
    public async Task Process_ValidQuery_ReturnsSuccessResultWithCompanies()
    {
        // Arrange
        var expectedCompanies = new List<Company>
        {
            Company.Create(CompanyId.Create(), this.faker.Company.CompanyName(), this.faker.Internet.Email()),
            Company.Create(CompanyId.Create(), this.faker.Company.CompanyName(), this.faker.Internet.Email()),
            Company.Create(CompanyId.Create(), this.faker.Company.CompanyName(), this.faker.Internet.Email())
        };

        var repository = Substitute.For<IGenericRepository<Company>>();
        repository.FindAllAsync(
                Arg.Any<FindOptions<Company>>(),
                cancellationToken: CancellationToken.None)
            .Returns(expectedCompanies.AsEnumerable());

        var loggerFactory = Substitute.For<ILoggerFactory>();
        var query = new CompanyFindAllQuery();
        var sut = new CompanyFindAllQueryHandler(loggerFactory, repository);

        // Act
        var response = await sut.Process(query, CancellationToken.None);

        // Assert
        response?.Result.ShouldNotBeNull().ShouldBeSuccess();
        response?.Result.Value.ShouldNotBeNull();
        response?.Result.Value.Count().ShouldBe(expectedCompanies.Count);

        await repository.Received(1).FindAllAsync(
            Arg.Any<FindOptions<Company>>(),
            cancellationToken: CancellationToken.None);
    }
}