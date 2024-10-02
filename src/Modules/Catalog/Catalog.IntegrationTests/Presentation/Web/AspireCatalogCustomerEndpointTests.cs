// // MIT-License
// // Copyright BridgingIT GmbH - All Rights Reserved
// // Use of this source code is governed by an MIT-style license that can be
// // found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license
//
// namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.IntegrationTests.Presentation;
//
// using System.Net.Mime;
// using System.Text.Json;
// using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
// using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
// using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;
// using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Domain;
//
// [IntegrationTest("Catalog:Presentation")]
// public class AspireCatalogCustomerEndpointTests(
//     ITestOutputHelper output,
//     AspirePresentationWebFixture fixture)
//     : IClassFixture<AspirePresentationWebFixture> // https://xunit.net/docs/shared-context#class-fixture
// {
//     [Theory]
//     [InlineData("api/tenants/[TENANTID]/catalog/customers")]
//     public async Task Get_SingleExisting_ReturnsOk(string route)
//     {
//         // Arrange
//         output.WriteLine($"Start Endpoint test for route: {route}");
//         var model = await this.PostCustomerCreate(route);
//
//         // Act
//         var client = fixture.CreateDefaultClient();
//         var response = await client
//             .GetAsync(route.Replace("[TENANTID]", model.TenantId) + $"/{model.Id}")
//             .AnyContext();
//         output.WriteLine($"Finish Endpoint test for route: {route} (status={(int)response.StatusCode})");
//
//         // Assert
//         response.Should().Be200Ok();
//         var responseModel = await response.Content.ReadAsAsync<CustomerModel>();
//         responseModel.ShouldNotBeNull();
//         responseModel.PersonName.Parts[0].Should().Be(model.PersonName.Parts[0]);
//         responseModel.PersonName.Parts[1].Should().Be(model.PersonName.Parts[1]);
//         responseModel.Email.Should().Be(model.Email);
//         output.WriteLine($"ResponseModel: {responseModel.DumpText()}");
//     }
//
//     private async Task<CustomerModel> PostCustomerCreate(string route)
//     {
//         TenantId[] tenantIds =
//         [
//             TenantIdFactory.CreateForName("Tenant_AcmeBooks"), TenantIdFactory.CreateForName("Tenant_TechBooks")
//         ];
//         var customer = CatalogSeedEntities.Customers.Create(tenantIds, DateTime.UtcNow.Ticks)[0];
//         var model = new CustomerModel
//         {
//             TenantId = customer.TenantId,
//             PersonName =
//                 new PersonFormalNameModel
//                 {
//                     Parts = customer.PersonName.Parts.ToArray(),
//                     Title = customer.PersonName.Title,
//                     Suffix = customer.PersonName.Suffix
//                 },
//             Email = customer.Email,
//             Address = new AddressModel
//             {
//                 Name = customer.Address.Name,
//                 Line1 = customer.Address.Line1,
//                 Line2 = customer.Address.Line2,
//                 PostalCode = customer.Address.PostalCode,
//                 City = customer.Address.City,
//                 Country = customer.Address.Country
//             }
//         };
//         var content = new StringContent(
//             JsonSerializer.Serialize(model, DefaultSystemTextJsonSerializerOptions.Create()),
//             Encoding.UTF8,
//             MediaTypeNames.Application.Json);
//         var client = fixture.CreateDefaultClient();
//         var response = await client
//             .PostAsync(route.Replace("[TENANTID]", model.TenantId), content)
//             .AnyContext();
//         response.EnsureSuccessStatusCode();
//
//         return await response.Content.ReadAsAsync<CustomerModel>();
//     }
// }