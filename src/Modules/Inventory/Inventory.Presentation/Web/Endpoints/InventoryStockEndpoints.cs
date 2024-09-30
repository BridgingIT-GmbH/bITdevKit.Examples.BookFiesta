// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Presentation.Web;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class InventoryStockEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/tenants/{tenantId}/inventory/stocks")
            .WithTags("Inventory");

        group.MapGet("/{id}", GetStock)
            .WithName("GetInventoryStock")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/", GetStocks)
            .WithName("GetInventoryStocks")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost("/", CreateStock)
            .WithName("CreateInventoryStock")
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<StockModel>, NotFound, ProblemHttpResult>> GetStock(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new StockFindOneQuery(tenantId, id))).Result;

        return result.Value == null ? TypedResults.NotFound() :
            result.IsSuccess ? TypedResults.Ok(mapper.Map<Stock, StockModel>(result.Value)) :
            TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<StockModel>>, ProblemHttpResult>> GetStocks(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId)
    {
        var result = (await mediator.Send(new StockFindAllQuery(tenantId))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Stock>, IEnumerable<StockModel>>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Created<StockModel>, ProblemHttpResult>> CreateStock(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromBody] StockModel model)
    {
        var result = (await mediator.Send(new StockCreateCommand(tenantId, model))).Result;

        return result.IsSuccess
            ? TypedResults.Created(
                $"api/tenants/{tenantId}/inventory/stocks/{result.Value.Id}",
                mapper.Map<Stock, StockModel>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }
}