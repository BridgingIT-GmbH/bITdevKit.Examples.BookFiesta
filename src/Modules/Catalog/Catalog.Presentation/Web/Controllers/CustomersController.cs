// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license
//
// namespace BridgingIT.DevKit.Examples.BookFiesta.Presentation.Web.Controllers;
//
// using System.Threading;
// using BridgingIT.DevKit.Common;
// using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
// using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
// using BridgingIT.DevKit.Presentation.Web;
// using MediatR;
// using Microsoft.AspNetCore.Mvc;
//
// [ApiController]
// [Route("api/tenants/{tenantId}/[controller]")]
// public class CustomersController(IMapper mapper, IMediator mediator) : ControllerBase // TODO: use the new IEndpoints from bitdevkit, see Maps below
// {
//     private readonly IMediator mediator = mediator;
//     private readonly IMapper mapper = mapper;
//
//     [HttpGet("{id}", Name = nameof(Get))]
//     public async Task<ActionResult<CustomerModel>> Get(string tenantId, string id, CancellationToken cancellationToken)
//     {
//         var result = (await this.mediator.Send(
//             new CustomerFindOneQuery(tenantId, id), cancellationToken)).Result;
//         return result.ToOkActionResult<Customer, CustomerModel>(this.mapper);
//     }
//
//     [HttpGet]
//     public async Task<ActionResult<ICollection<CustomerModel>>> GetAll(string tenantId, CancellationToken cancellationToken)
//     {
//         var result = (await this.mediator.Send(
//             new CustomerFindAllQuery(tenantId), cancellationToken)).Result;
//         return result.ToOkActionResult<Customer, CustomerModel>(this.mapper);
//     }
//
//     [HttpPost]
//     public async Task<ActionResult<CustomerModel>> PostAsync(string tenantId, [FromBody] CustomerModel model, CancellationToken cancellationToken)
//     {
//         if (tenantId != model.TenantId)
//         {
//             return new BadRequestObjectResult(null);
//         }
//
//         var result = (await this.mediator.Send(
//             this.mapper.Map<CustomerModel, CustomerCreateCommand>(model), cancellationToken)).Result;
//         return result.ToCreatedActionResult<Customer, CustomerModel>(this.mapper, nameof(this.Get), new { id = result.Value?.Id });
//     }
//
//     [HttpPut("{id}")]
//     public async Task<ActionResult<CustomerModel>> PutAsync(string tenantId, string id, [FromBody] CustomerModel model, CancellationToken cancellationToken)
//     {
//         if (tenantId != model.TenantId || id != model.Id)
//         {
//             return new BadRequestObjectResult(null);
//         }
//
//         var result = (await this.mediator.Send(
//             this.mapper.Map<CustomerModel, CustomerUpdateCommand>(model), cancellationToken)).Result;
//         return result.ToUpdatedActionResult<Customer, CustomerModel>(this.mapper, nameof(this.Get), new { id = result.Value?.Id });
//     }
//
//     [HttpDelete("{id}")]
//     public async Task<ActionResult<CustomerModel>> DeleteAsync(string tenantId, string id, CancellationToken cancellationToken)
//     {
//         var result = (await this.mediator.Send(new CustomerDeleteCommand(tenantId, id), cancellationToken)).Result;
//         return result.ToDeletedActionResult<CustomerModel>(); // TODO: remove generic CustomerModel
//     }
// }
//
// //app.MapGet("/api/customers/{id}", async(string id, IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
// //{
// //    var result = (await mediator.Send(new CustomerFindOneQuery(id), cancellationToken)).Result;
// //    return result.ToOkActionResult<Customer, CustomerModel>(mapper);
// //}).WithName("GetCustomer");
//
// //// Endpoint for GetAll action
// //app.MapGet("/api/customers", async (IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
// //{
// //    var result = (await mediator.Send(new CustomerFindAllQuery(), cancellationToken)).Result;
// //    return result.ToOkActionResult<Customer, CustomerModel>(mapper);
// //});
//
// //// Endpoint for PostAsync action
// //app.MapPost("/api/customers", async (CustomerModel model, IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
// //{
// //    var result = (await mediator.Send(mapper.Map<CustomerModel, CustomerCreateCommand>(model), cancellationToken)).Result;
// //    return result.ToCreatedActionResult<Customer, CustomerModel>(mapper, "GetCustomer", new { id = result.Value?.Id });
// //});
//
// //// Endpoint for PutAsync action
// //app.MapPut("/api/customers/{id}", async (string id, CustomerModel model, IMediator mediator, IMapper mapper, CancellationToken cancellationToken) =>
// //{
// //    var result = (await mediator.Send(mapper.Map<CustomerModel, CustomerUpdateCommand>(model), cancellationToken)).Result;
// //    return result.ToUpdatedActionResult<Customer, CustomerModel>(mapper, "GetCustomer", new { id = result.Value?.Id });
// //});
//
// //// Endpoint for DeleteAsync action
// //app.MapDelete("/api/customers/{id}", async (string id, IMediator mediator, CancellationToken cancellationToken) =>
// //{
// //    var result = (await mediator.Send(new CustomerDeleteCommand { Id = id }, cancellationToken)).Result;
// //    return result.ToDeletedActionResult<CustomerModel>();
// //});

