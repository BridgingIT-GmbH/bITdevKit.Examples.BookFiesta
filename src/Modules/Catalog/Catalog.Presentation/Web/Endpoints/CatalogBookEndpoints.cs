// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Presentation.Web;

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

public class CatalogBookEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/tenants/{tenantId}/catalog/books")
            .WithGroupName("Catalog/Books")
            .WithTags("Catalog");

        group.MapGet("/{id}", GetBook)
            .WithName("GetCatalogBook")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet("/", GetBooks)
            .WithName("GetCatalogBooks")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost("/", CreateBook)
            .WithName("CreateCatalogBook")
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPut("/{id}", UpdateBook)
            .WithName("UpdateCatalogBook")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapDelete("/{id}", DeleteBook)
            .WithName("DeleteCatalogBook")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<BookModel>, NotFound, ProblemHttpResult>> GetBook(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new BookFindOneQuery(tenantId, id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Book, BookModel>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<BookModel>>, ProblemHttpResult>> GetBooks(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId)
    {
        var result = (await mediator.Send(new BookFindAllQuery(tenantId))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Book>, IEnumerable<BookModel>>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Created<BookModel>, ProblemHttpResult>> CreateBook(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromBody] BookModel model)
    {
        var result = (await mediator.Send(
            new BookCreateOrUpdateCommand(tenantId, model, UpsertOperationType.Create))).Result;

        return result.IsSuccess
            ? TypedResults.Created(
                $"api/tenants/{tenantId}/catalog/books/{result.Value.Id}",
                mapper.Map<Book, BookModel>(result.Value))
            : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok<BookModel>, NotFound, ProblemHttpResult>> UpdateBook(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id,
        [FromBody] BookModel model)
    {
        var result = (await mediator.Send(
            new BookCreateOrUpdateCommand(tenantId, model, UpsertOperationType.Update))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Book, BookModel>(result.Value))
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }

    private static async Task<Results<Ok, NotFound, ProblemHttpResult>> DeleteBook(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new BookDeleteCommand(tenantId, id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok()
                : TypedResults.Problem(result.ToString(), statusCode: 400);
    }
}