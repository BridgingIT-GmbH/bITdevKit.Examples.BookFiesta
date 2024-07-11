namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Presentation.Web;

using System.Collections.Generic;
using System.Net;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookStore.Application;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using BridgingIT.DevKit.Examples.BookStore.Presentation;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class CatalogCategoryEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/catalog/categories")
            .WithTags("Catalog");

        group.MapGet("/{id}", async Task<Results<Ok<CategoryModel>, NotFound, ProblemHttpResult>>(
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper,
            [FromRoute] string id) =>
        {
            var result = (await mediator.Send(new CategoryFindOneQuery(id))).Result;

            return (result.Value == null) ? TypedResults.NotFound() : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Category, CategoryModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
        }).WithName("GetCatalogCategory")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/{id}/books", async Task<Results<Ok<IEnumerable<BookModel>>, NotFound, ProblemHttpResult>>(
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper,
            [FromRoute] string id) =>
        {
            var result = (await mediator.Send(new BookFindAllForCategoryQuery(id))).Result;

            return result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Book, BookModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
        }).WithName("GetCatalogCategoryBooks")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-8.0
        group.MapGet(string.Empty, async Task<Results<Ok<IEnumerable<CategoryModel>>, ProblemHttpResult>>(
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper) =>
        {
            var result = (await mediator.Send(new CategoryFindAllQuery())).Result;

            return result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Category, CategoryModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
        }).WithName("GetCatalogCategories")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }
}