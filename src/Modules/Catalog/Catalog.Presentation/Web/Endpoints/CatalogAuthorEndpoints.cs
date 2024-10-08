// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.AuthorFiesta.Modules.Catalog.Presentation.Web;

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

public class CatalogAuthorEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/tenants/{tenantId}/catalog/authors")
            .WithGroupName("Catalog/Authors")
            .WithTags("Catalog");

        group.MapGet("/{id}", GetAuthor)
            .WithName("GetCatalogAuthor")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/", GetAuthors)
            .WithName("GetCatalogAuthors")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost("/", CreateAuthor)
            .WithName("CreateCatalogAuthor")
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<AuthorModel>, NotFound, ProblemHttpResult>> GetAuthor(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new AuthorFindOneQuery(tenantId, id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Author, AuthorModel>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<AuthorModel>>, ProblemHttpResult>> GetAuthors(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId)
    {
        var result = (await mediator.Send(new AuthorFindAllQuery(tenantId))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Author>, IEnumerable<AuthorModel>>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static Task<Results<Created<AuthorModel>, ProblemHttpResult>> CreateAuthor(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromBody] AuthorModel model)
    {
        throw new NotImplementedException();
        // var result = (await mediator.Send(new AuthorCreateCommand(tenantId, model))).Result;
        //
        // return result.IsSuccess
        //     ? TypedResults.Created(
        //         $"api/tenants/{tenantId}/catalog/books/{result.Value.Id}",
        //         mapper.Map<Author, AuthorModel>(result.Value))
        //     : TypedResults.Problem(result.ToString(), statusCode: 400);
    }
}