namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Presentation.Web;

using System.Collections.Generic;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Application;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class CatalogBookEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/tenants/{tenantId}/catalog/books")
            .WithTags("Catalog");

        group.MapGet("/{id}", async Task<Results<Ok<BookModel>, NotFound, ProblemHttpResult>> (
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper,
            [FromRoute] string tenantId,
            [FromRoute] string id) =>
        {
            var result = (await mediator.Send(new BookFindOneQuery(tenantId, id))).Result;

            return (result.Value == null) ? TypedResults.NotFound() : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Book, BookModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
        }).WithName("GetCatalogBook")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet(string.Empty, async Task<Results<Ok<IEnumerable<BookModel>>, ProblemHttpResult>> (
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper,
            [FromRoute] string tenantId) =>
        {
            var result = (await mediator.Send(new BookFindAllQuery(tenantId))).Result;

            return result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Book, BookModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
        }).WithName("GetCatalogBooks")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost(string.Empty, async Task<Results<Created<BookModel>, ProblemHttpResult>> (
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper,
            [FromRoute] string tenantId,
            [FromBody] BookModel model) =>
        {
            var result = (await mediator.Send(new BookCreateCommand(tenantId, model))).Result;

            return result.IsSuccess
                ? TypedResults.Created($"api/tenants/{tenantId}/catalog/books/{result.Value.Id}",
                                       mapper.Map<Book, BookModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "),
                                       statusCode: 400);
        }).WithName("CreateCatalogBook")
            .Produces<BookModel>(201)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }
}
