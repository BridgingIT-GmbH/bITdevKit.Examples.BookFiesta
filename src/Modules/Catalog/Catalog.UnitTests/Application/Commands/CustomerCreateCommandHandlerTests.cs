// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.UnitTests.Application;

using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[UnitTest("Catalog:Application")]
public class CustomerCreateCommandHandlerTests
{
    [Fact]
    public async Task Process_ValidRequest_SuccessResult()
    {
        // Arrange
        var repository = Substitute.For<IGenericRepository<Customer>>();
        TenantId[] tenantIds = [TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")];
        var customer = CatalogSeedEntities.Customers.Create(tenantIds, DateTime.UtcNow.Ticks)[0];
        var model = new CustomerModel
        {
            TenantId = customer.TenantId,
            PersonName = new PersonFormalNameModel
            {
                Parts = customer.PersonName.Parts.ToArray(),
                Title = customer.PersonName.Title,
                Suffix = customer.PersonName.Suffix
            },
            Email = customer.Email,
            Address = new AddressModel
            {
                Name = customer.Address.Name,
                Line1 = customer.Address.Line1,
                Line2 = customer.Address.Line2,
                PostalCode = customer.Address.PostalCode,
                City = customer.Address.City,
                Country = customer.Address.Country
            },
        };
        var command = new CustomerCreateCommand(tenantIds[0], model);
        var sut = new CustomerCreateCommandHandler(Substitute.For<ILoggerFactory>(), repository);

        // Act
        var response = await sut.Process(command, CancellationToken.None);

        // Assert
        response?.Result.ShouldNotBeNull();
        response.Result.Value.PersonName.Parts.ToArray()[0].ShouldBe(command.Model.PersonName.Parts[0]);
        response.Result.Value.PersonName.Parts.ToArray()[1].ShouldBe(command.Model.PersonName.Parts[1]);
        await repository.Received(1).InsertAsync(Arg.Any<Customer>(), CancellationToken.None);
    }
}