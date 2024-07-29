// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.UnitTests.Application;

using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[UnitTest("GettingStarted.Application")]
public class CustomerCreateCommandHandlerTests
{
    [Fact]
    public async Task Process_ValidRequest_SuccessResult()
    {
        // Arrange
        var repository = Substitute.For<IGenericRepository<Customer>>();
        TenantId[] tenantIds = [TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")];
        var customer = CatalogSeedModels.Customers.Create(tenantIds, DateTime.UtcNow.Ticks)[0];
        var command = new CustomerCreateCommand(tenantIds[0]) { FirstName = customer.FirstName, LastName = customer.LastName, Email = customer.Email, AddressLine1 = customer.Address.Line1, AddressLine2 = customer.Address.Line2, AddressPostalCode = customer.Address.PostalCode, AddressCity = customer.Address.City, AddressCountry = customer.Address.Country };
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
