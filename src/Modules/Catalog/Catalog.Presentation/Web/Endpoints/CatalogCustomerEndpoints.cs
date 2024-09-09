// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Presentation.Web;

using System.Collections.Generic;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class CatalogCustomerEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/tenants/{tenantId}/catalog/customers")
            .WithTags("Catalog");

        group.MapGet("/{id}", GetCustomer).WithName("GetCatalogCustomer")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet(string.Empty, GetCustomers).WithName("GetCatalogCustomers")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost(string.Empty, CreateCustomer).WithName("CreateCatalogCustomer")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPut("/{id}", UpdateCustomer).WithName("UpdateCatalogCustomer")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapDelete("/{id}", DeleteCustomer).WithName("DeleteCatalogCustomer")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<CustomerModel>, NotFound, ProblemHttpResult>> GetCustomer(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new CustomerFindOneQuery(tenantId, id))).Result;

        return result.Value == null ? TypedResults.NotFound() : result.IsSuccess
            ? TypedResults.Ok(mapper.Map<Customer, CustomerModel>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<CustomerModel>>, ProblemHttpResult>> GetCustomers(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId)
    {
        var result = (await mediator.Send(new CustomerFindAllQuery(tenantId))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Customer>, IEnumerable<CustomerModel>>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Created<CustomerModel>, ProblemHttpResult>> CreateCustomer(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromBody] CustomerModel model)
    {
        var result = (await mediator.Send(new CustomerCreateCommand(tenantId, model))).Result;

        return result.IsSuccess
            ? TypedResults.Created($"api/tenants/{tenantId}/catalog/customers/{result.Value.Id}",
                                   mapper.Map<Customer, CustomerModel>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "),
                                   statusCode: 400);
    }

    private static async Task<Results<Ok<CustomerModel>, ProblemHttpResult>> UpdateCustomer(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id,
        [FromBody] CustomerModel model)
    {
        var result = (await mediator.Send(new CustomerUpdateCommand(tenantId, model))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<Customer, CustomerModel>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "),
                                   statusCode: 400);
    }

    private static async Task<Results<Ok, NotFound, ProblemHttpResult>> DeleteCustomer(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new CustomerDeleteCommand(tenantId, id))).Result;

        return result.HasError<EntityNotFoundResultError>() ? TypedResults.NotFound() : result.IsSuccess
            ? TypedResults.Ok()
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }
}