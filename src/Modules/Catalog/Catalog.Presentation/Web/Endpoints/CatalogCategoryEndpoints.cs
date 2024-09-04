// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

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

public class CatalogCategoryEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/tenants/{tenantId}/catalog/categories")
            .WithTags("Catalog");

        group.MapGet("/{id}", GetCategory).WithName("GetCatalogCategory")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/{id}/books", GetCategoryBooks).WithName("GetCatalogCategoryBooks")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet(string.Empty, GetCategories).WithName("GetCatalogCategories")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<CategoryModel>, NotFound, ProblemHttpResult>> GetCategory(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new CategoryFindOneQuery(tenantId, id))).Result;

        return result.Value == null ? TypedResults.NotFound() : result.IsSuccess
            ? TypedResults.Ok(mapper.Map<Category, CategoryModel>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<BookModel>>, NotFound, ProblemHttpResult>> GetCategoryBooks(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new BookFindAllForCategoryQuery(tenantId, id))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Book>, IEnumerable<BookModel>>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<CategoryModel>>, ProblemHttpResult>> GetCategories(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId)
    {
        var result = (await mediator.Send(new CategoryFindAllQuery(tenantId))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Category>, IEnumerable<CategoryModel>>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }
}