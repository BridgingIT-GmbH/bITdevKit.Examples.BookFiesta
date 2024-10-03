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

public class InventoryStockSnapshotEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/tenants/{tenantId}/inventory/stocks/{stockId}/stocksnapshots")
            .WithTags("Inventory");

        group.MapGet("/{id}", GetStockSnapshot)
            .WithName("GetInventoryStockSnapshot")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/", GetStockSnapshots)
            .WithName("GetInventoryStockSnapshots")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost("/", CreateStockSnapshot)
            .WithName("CreateInventoryStockSnapshot")
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<StockSnapshotModel>, NotFound, ProblemHttpResult>> GetStockSnapshot(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string stockId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(
            new StockSnapshotFindOneQuery(tenantId, stockId, id))).Result;

        return result.Value == null ? TypedResults.NotFound() :
            result.IsSuccess ? TypedResults.Ok(mapper.Map<StockSnapshot, StockSnapshotModel>(result.Value)) :
            TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<StockSnapshotModel>>, ProblemHttpResult>> GetStockSnapshots(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string stockId)
    {
        var result = (await mediator.Send(
            new StockSnapshotFindAllQuery(tenantId, stockId))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<StockSnapshot>, IEnumerable<StockSnapshotModel>>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Created<StockSnapshotModel>, ProblemHttpResult>> CreateStockSnapshot(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string stockId)
    {
        var result = (await mediator.Send(
            new StockSnapshotCreateCommand(tenantId, stockId))).Result;

        return result.IsSuccess
            ? TypedResults.Created(
                $"api/tenants/{tenantId}/inventory/stocks/{stockId}/stocksnapshots/{result.Value.Id}",
                mapper.Map<StockSnapshot, StockSnapshotModel>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }
}