// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.UnitTests.Application;

using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookStore.Application;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

[UnitTest("GettingStarted.Application")]
public class CustomerCreateCommandHandlerTests
{
    [Fact]
    public async Task Process_ValidRequest_SuccessResult()
    {
        // Arrange
        var repository = Substitute.For<IGenericRepository<Customer>>();
        var companies = CoreSeedModels.Companies.Create(DateTime.UtcNow.Ticks);
        var tenants = CoreSeedModels.Tenants.Create(companies, DateTime.UtcNow.Ticks);
        var customer = CoreSeedModels.Customers.Create(tenants, DateTime.UtcNow.Ticks)[0];
        var command = new CustomerCreateCommand { FirstName = customer.FirstName, LastName = customer.LastName, Email = customer.Email, AddressLine1 = customer.Address.Line1, AddressLine2 = customer.Address.Line2, AddressPostalCode = customer.Address.PostalCode, AddressCity = customer.Address.City, AddressCountry = customer.Address.Country };
        var sut = new CustomerCreateCommandHandler(Substitute.For<ILoggerFactory>(), repository);

        // Act
        var response = await sut.Process(command, CancellationToken.None);

        // Assert
        response?.Result.ShouldNotBeNull();
        response.Result.Value.FirstName.ShouldBe(command.FirstName);
        response.Result.Value.LastName.ShouldBe(command.LastName);
        await repository.Received(1).InsertAsync(Arg.Any<Customer>(), CancellationToken.None);
    }
}
