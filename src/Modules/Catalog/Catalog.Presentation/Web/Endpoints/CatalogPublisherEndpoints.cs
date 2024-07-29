namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Presentation.Web;

using System.Collections.Generic;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Catalog.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class CatalogPublisherEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/tenants/{tenantId}catalog/publishers")
            .WithTags("Catalog");

        group.MapGet("/{id}", async Task<Results<Ok<PublisherModel>, NotFound, ProblemHttpResult>> (
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper,
            [FromRoute] string tenantId,
            [FromRoute] string id) =>
        {
            var result = (await mediator.Send(new PublisherFindOneQuery(tenantId, id))).Result;

            return (result.Value == null) ? TypedResults.NotFound() : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Publisher, PublisherModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
        }).WithName("GetCatalogPublisher")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/{id}/books", async Task<Results<Ok<IEnumerable<BookModel>>, NotFound, ProblemHttpResult>> (
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper,
            [FromRoute] string tenantId,
            [FromRoute] string id) =>
        {
            var result = (await mediator.Send(new BookFindAllForPublisherQuery(tenantId, id))).Result;

            return result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Book, BookModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
        }).WithName("GetCatalogPublisherBooks")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-8.0
        group.MapGet(string.Empty, async Task<Results<Ok<IEnumerable<PublisherModel>>, ProblemHttpResult>> (
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper,
            [FromRoute] string tenantId) =>
        {
            var result = (await mediator.Send(new PublisherFindAllQuery(tenantId))).Result;

            return result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Publisher, PublisherModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
        }).WithName("GetCatalogPublishers")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }
}