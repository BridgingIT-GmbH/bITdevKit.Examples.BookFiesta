// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.IntegrationTests.Presentation.Web;

using System.Text.Json;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;
using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;

[IntegrationTest("Presentation.Web")]
public class OrganizationCompanyEndpointsTests(ITestOutputHelper output, CustomWebApplicationFactoryFixture<Program> fixture)
    : IClassFixture<CustomWebApplicationFactoryFixture<Program>> // https://xunit.net/docs/shared-context#class-fixture
{
    private readonly CustomWebApplicationFactoryFixture<Program> fixture = fixture.WithOutput(output);

    [Theory]
    [InlineData("api/organization/companies")]
    public async Task Get_SingleExisting_ReturnsOk(string route)
    {
        // Arrange
        this.fixture.Output.WriteLine($"Start Endpoint test for route: {route}");
        var model = await this.PostCompanyCreate(route);

        // Act
        var response = await this.fixture.CreateClient()
            .GetAsync(route + $"/{model.Id}").AnyContext();
        this.fixture.Output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");

        // Assert
        response.Should().Be200Ok();
        var responseModel = await response.Content.ReadAsAsync<CompanyModel>();
        responseModel.ShouldNotBeNull();
        responseModel.Should().BeEquivalentTo(model);
        this.fixture.Output.WriteLine($"ResponseModel: {responseModel.DumpText()}");
    }

    [Theory]
    [InlineData("api/organization/companies")]
    public async Task Get_SingleNotExisting_ReturnsNotFound(string route)
    {
        // Arrange
        this.fixture.Output.WriteLine($"Start Endpoint test for route: {route}");

        // Act
        var response = await this.fixture.CreateClient()
            .GetAsync(route + $"/{Guid.NewGuid()}").AnyContext();
        this.fixture.Output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");

        // Assert
        response.Should().Be404NotFound();
    }

    [Theory]
    [InlineData("api/organization/companies")]
    public async Task Get_MultipleExisting_ReturnsOk(string route)
    {
        // Arrange & Act
        this.fixture.Output.WriteLine($"Start Endpoint test for route: {route}");
        var model = await this.PostCompanyCreate(route);

        var response = await this.fixture.CreateClient()
            .GetAsync(route).AnyContext();
        this.fixture.Output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");

        // Assert
        response.Should().Be200Ok();
        var responseModel = await response.Content.ReadAsAsync<ICollection<CompanyModel>>();
        responseModel.ShouldNotBeNull();
        responseModel.Should().Contain(c => c.Name == model.Name);
        responseModel.Should().Contain(c => c.RegistrationNumber == model.RegistrationNumber);
        responseModel.Should().Contain(c => c.ContactEmail == model.ContactEmail);
        this.fixture.Output.WriteLine($"ResponseModel: {responseModel.DumpText()}");
    }

    [Theory]
    [InlineData("api/organization/companies")]
    public async Task Post_ValidModel_ReturnsCreated(string route)
    {
        // Arrange
        this.fixture.Output.WriteLine($"Start Endpoint test for route: {route}");
        TenantId[] tenantIds = [
            TenantIdFactory.CreateForName("Tenant_AcmeBooks"),
            TenantIdFactory.CreateForName("Tenant_TechBooks")
            ];
        var company = OrganizationSeedEntities.Companies.Create(DateTime.UtcNow.Ticks)[0];
        var model = new CompanyModel
        {
            Name = company.Name,
            RegistrationNumber = company.RegistrationNumber,
            ContactEmail = company.ContactEmail,
            Address = new AddressModel
            {
                Name = company.Address.Name ?? company.Name,
                Line1 = company.Address.Line1,
                Line2 = company.Address.Line2,
                PostalCode = company.Address.PostalCode,
                City = company.Address.City,
                Country = company.Address.Country
            },
        };
        var content = new StringContent(
            JsonSerializer.Serialize(model, DefaultSystemTextJsonSerializerOptions.Create()), Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);

        // Act
        var response = await this.fixture.CreateClient()
            .PostAsync(route, content).AnyContext();
        this.fixture.Output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");

        // Assert
        response.Should().Be201Created();
        response.Headers.Location.Should().NotBeNull();
        var responseModel = await response.Content.ReadAsAsync<CompanyModel>();
        responseModel.ShouldNotBeNull();
        responseModel.Should().BeEquivalentTo(model, options => options.Excluding(m => m.Id));
        this.fixture.Output.WriteLine($"ResponseModel: {responseModel.DumpText()}");
    }

    [Theory]
    [InlineData("api/organization/companies")]
    public async Task Post_InvalidModel_ReturnsBadRequest(string route)
    {
        // Arrange
        this.fixture.Output.WriteLine($"Start Endpoint test for route: {route}");
        var company = OrganizationSeedEntities.Companies.Create(DateTime.UtcNow.Ticks)[0];
        var model = new CompanyModel
        {
            Name = string.Empty,
            RegistrationNumber = string.Empty,
            ContactEmail = string.Empty,
            Address = new AddressModel
            {
                Name = company.Address.Name ?? company.Name,
                Line1 = company.Address.Line1,
                Line2 = company.Address.Line2,
                PostalCode = company.Address.PostalCode,
                City = company.Address.City,
                Country = company.Address.Country
            },
        };
        var content = new StringContent(
            JsonSerializer.Serialize(model, DefaultSystemTextJsonSerializerOptions.Create()), Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);

        // Act
        var response = await this.fixture.CreateClient()
            .PostAsync(route, content).AnyContext();
        this.fixture.Output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");

        // Assert
        response.Should().Be400BadRequest();
        var responseModel = await response.Content.ReadAsStringAsync();
        responseModel.Should().Contain("ValidationException");
        responseModel.Should().Contain(nameof(model.Name));
        responseModel.Should().Contain(nameof(model.RegistrationNumber));
        responseModel.Should().Contain(nameof(model.ContactEmail));
    }

    [Theory]
    [InlineData("api/organization/companies")]
    public async Task Put_ValidModel_ReturnsOk(string route)
    {
        // Arrange
        this.fixture.Output.WriteLine($"Start Endpoint test for route: {route}");
        var model = await this.PostCompanyCreate(route);
        model.Name += "changed";
        model.RegistrationNumber += "changed";
        var content = new StringContent(
            JsonSerializer.Serialize(model, DefaultSystemTextJsonSerializerOptions.Create()), Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);

        // Act
        var response = await this.fixture.CreateClient()
            .PutAsync(route + $"/{model.Id}", content).AnyContext();
        this.fixture.Output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");

        // Assert
        response.Should().Be200Ok();
        var responseModel = await response.Content.ReadAsAsync<CompanyModel>();
        responseModel.ShouldNotBeNull();
        responseModel.Should().BeEquivalentTo(model);
        this.fixture.Output.WriteLine($"ResponseModel: {responseModel.DumpText()}");
    }

    [Theory]
    [InlineData("api/organization/companies")]
    public async Task Delete_ValidId_ReturnsOk(string route)
    {
        // Arrange
        this.fixture.Output.WriteLine($"Start Endpoint test for route: {route}");
        var model = await this.PostCompanyCreate(route);

        // Act
        var response = await this.fixture.CreateClient()
            .DeleteAsync(route + $"/{model.Id}").AnyContext();
        this.fixture.Output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");

        // Assert
        response.Should().Be200Ok();
        response.Headers.Location.Should().BeNull();
    }

    private async Task<CompanyModel> PostCompanyCreate(string route)
    {
        var company = OrganizationSeedEntities.Companies.Create(DateTime.UtcNow.Ticks)[0];
        var model = new CompanyModel
        {
            Name = company.Name,
            RegistrationNumber = company.RegistrationNumber,
            ContactEmail = company.ContactEmail,
            ContactPhone = company.ContactPhone,
            Website = company.Website,
            VatNumber = company.VatNumber,
            Address = new AddressModel
            {
                Name = company.Address.Name,
                Line1 = company.Address.Line1,
                Line2 = company.Address.Line2,
                PostalCode = company.Address.PostalCode,
                City = company.Address.City,
                Country = company.Address.Country
            },
        };
        var content = new StringContent(
            JsonSerializer.Serialize(model, DefaultSystemTextJsonSerializerOptions.Create()), Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);
        var response = await this.fixture.CreateClient()
            .PostAsync(route, content).AnyContext();
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<CompanyModel>();
    }
}