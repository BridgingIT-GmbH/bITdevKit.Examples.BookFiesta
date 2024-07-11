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