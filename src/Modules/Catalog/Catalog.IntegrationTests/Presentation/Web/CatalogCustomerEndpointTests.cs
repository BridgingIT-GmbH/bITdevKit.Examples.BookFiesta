// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.IntegrationTests.Presentation.Web;

using System.Text.Json;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[IntegrationTest("Presentation.Web")]
public class CatalogCustomerEndpointTests(ITestOutputHelper output, CustomWebApplicationFactoryFixture<Program> fixture) : IClassFixture<CustomWebApplicationFactoryFixture<Program>> // https://xunit.net/docs/shared-context#class-fixture
{
    private readonly CustomWebApplicationFactoryFixture<Program> fixture = fixture.WithOutput(output);

    [Theory]
    [InlineData("api/tenants/[TENANTID]/catalog/customers")]
    public async Task Get_SingleExisting_ReturnsOk(string route)
    {
        // Arrange
        this.fixture.Output.WriteLine($"Start Endpoint test for route: {route}");
        var model = await this.PostCustomerCreate(route);

        // Act
        var response = await this.fixture.CreateClient()
            .GetAsync(route.Replace("[TENANTID]", model.TenantId) + $"/{model.Id}").AnyContext();
        this.fixture.Output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");

        // Assert
        response.Should().Be200Ok(); // https://github.com/adrianiftode/FluentAssertions.Web
        response.Should().MatchInContent($"*{model.PersonName.Parts[0]}*");
        response.Should().MatchInContent($"*{model.PersonName.Parts[1]}*");
        response.Should().MatchInContent($"*{model.Email}*");
        var responseModel = await response.Content.ReadAsAsync<CustomerModel>();
        responseModel.ShouldNotBeNull();
        this.fixture.Output.WriteLine($"ResponseModel: {responseModel.DumpText()}");
    }

    [Theory]
    [InlineData("api/tenants/[TENANTID]/catalog/customers")]
    public async Task Get_SingleNotExisting_ReturnsNotFound(string route)
    {
        // Arrange
        this.fixture.Output.WriteLine($"Start Endpoint test for route: {route}");
        TenantId[] tenantIds = [TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")];

        // Act
        var response = await this.fixture.CreateClient()
            .GetAsync(route.Replace("[TENANTID]", tenantIds[0]) + $"/{Guid.NewGuid()}").AnyContext();
        this.fixture.Output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");

        // Assert
        response.Should().Be404NotFound(); // https://github.com/adrianiftode/FluentAssertions.Web
    }

    [Theory]
    [InlineData("api/tenants/[TENANTID]/catalog/customers")]
    public async Task Get_MultipleExisting_ReturnsOk(string route)
    {
        // Arrange & Act
        this.fixture.Output.WriteLine($"Start Endpoint test for route: {route}");
        var model = await this.PostCustomerCreate(route);

        var response = await this.fixture.CreateClient()
            .GetAsync(route.Replace("[TENANTID]", model.TenantId)).AnyContext();
        this.fixture.Output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");

        // Assert
        response.Should().Be200Ok(); // https://github.com/adrianiftode/FluentAssertions.Web
        response.Should().Satisfy<ICollection<CustomerModel>>(
            model =>
            {
                model.ShouldNotBeNull();
            });
        response.Should().MatchInContent($"*{model.PersonName.Parts[0]}*");
        response.Should().MatchInContent($"*{model.PersonName.Parts[1]}*");
        response.Should().MatchInContent($"*{model.Email}*");
        var responseModel = await response.Content.ReadAsAsync<ICollection<CustomerModel>>();
        responseModel.ShouldNotBeNull();
        this.fixture.Output.WriteLine($"ResponseModel: {responseModel.DumpText()}");
    }

    [Theory]
    [InlineData("api/tenants/[TENANTID]/catalog/customers")]
    public async Task Post_ValidModel_ReturnsCreated(string route)
    {
        // Arrange
        this.fixture.Output.WriteLine($"Start Endpoint test for route: {route}");
        TenantId[] tenantIds = [TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")];
        var customer = CatalogSeedModels.Customers.Create(tenantIds, DateTime.UtcNow.Ticks)[0];
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
        var content = new StringContent(
            JsonSerializer.Serialize(model, DefaultSystemTextJsonSerializerOptions.Create()), Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);

        // Act
        var response = await this.fixture.CreateClient()
            .PostAsync(route.Replace("[TENANTID]", model.TenantId), content).AnyContext();
        this.fixture.Output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");

        // Assert
        response.Should().Be201Created(); // https://github.com/adrianiftode/FluentAssertions.Web
        response.Headers.Location.Should().NotBeNull();
        var responseModel = await response.Content.ReadAsAsync<CustomerModel>();
        responseModel.ShouldNotBeNull();
        this.fixture.Output.WriteLine($"ResponseModel: {responseModel.DumpText()}");
    }

    [Theory]
    [InlineData("api/tenants/[TENANTID]/catalog/customers")]
    public async Task Post_InvalidEntity_ReturnsBadRequest(string route)
    {
        // Arrange
        this.fixture.Output.WriteLine($"Start Endpoint test for route: {route}");
        TenantId[] tenantIds = [TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")];
        var customer = CatalogSeedModels.Customers.Create(tenantIds, DateTime.UtcNow.Ticks)[0];
        var model = new CustomerModel
        {
            TenantId = customer.TenantId,
            PersonName = new PersonFormalNameModel
            {
                Parts = customer.PersonName.Parts.ToArray(),
                Title = customer.PersonName.Title,
                Suffix = customer.PersonName.Suffix
            },
            Email = string.Empty,
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
        var content = new StringContent(
            JsonSerializer.Serialize(model, DefaultSystemTextJsonSerializerOptions.Create()), Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);

        // Act
        var response = await this.fixture.CreateClient()
            .PostAsync(route.Replace("[TENANTID]", model.TenantId), content).AnyContext();
        this.fixture.Output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");

        // Assert
        response.Should().Be400BadRequest(); // https://github.com/adrianiftode/FluentAssertions.Web
        response.Should().MatchInContent("*[ValidationException]*");
    }

    [Theory]
    [InlineData("api/tenants/[TENANTID]/catalog/customers")]
    public async Task Put_ValidModel_ReturnsOk(string route)
    {
        // Arrange
        this.fixture.Output.WriteLine($"Start Endpoint test for route: {route}");
        var model = await this.PostCustomerCreate(route);
        model.PersonName.Parts[0] += "changedA";
        model.PersonName.Parts[1] += "changedB";
        var content = new StringContent(
            JsonSerializer.Serialize(model, DefaultSystemTextJsonSerializerOptions.Create()), Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);

        // Act
        var response = await this.fixture.CreateClient()
            .PutAsync(route.Replace("[TENANTID]", model.TenantId) + $"/{model.Id}", content).AnyContext();
        this.fixture.Output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");

        // Assert
        response.Should().Be200Ok(); // https://github.com/adrianiftode/FluentAssertions.Web
        response.Headers.Location.Should().BeNull();
        response.Should().MatchInContent($"*{model.PersonName.Parts[0]}*");
        response.Should().MatchInContent($"*{model.PersonName.Parts[1]}*");
        var responseModel = await response.Content.ReadAsAsync<CustomerModel>();
        responseModel.ShouldNotBeNull();
        this.fixture.Output.WriteLine($"ResponseModel: {responseModel.DumpText()}");
    }

    [Theory]
    [InlineData("api/tenants/[TENANTID]/catalog/customers")]
    public async Task Delete_ValidId_ReturnsOk(string route)
    {
        // Arrange
        this.fixture.Output.WriteLine($"Start Endpoint test for route: {route}");
        var model = await this.PostCustomerCreate(route);

        // Act
        var response = await this.fixture.CreateClient()
            .DeleteAsync(route.Replace("[TENANTID]", model.TenantId) + $"/{model.Id}").AnyContext();
        this.fixture.Output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");

        // Assert
        response.Should().Be200Ok(); // https://github.com/adrianiftode/FluentAssertions.Web
        response.Headers.Location.Should().BeNull();
    }

    private async Task<CustomerModel> PostCustomerCreate(string route)
    {
        TenantId[] tenantIds = [TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")];
        var customer = CatalogSeedModels.Customers.Create(tenantIds, DateTime.UtcNow.Ticks)[0];
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
        var content = new StringContent(
            JsonSerializer.Serialize(model, DefaultSystemTextJsonSerializerOptions.Create()), Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);
        var response = await this.fixture.CreateClient()
            .PostAsync(route.Replace("[TENANTID]", model.TenantId), content).AnyContext();
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<CustomerModel>();
    }
}