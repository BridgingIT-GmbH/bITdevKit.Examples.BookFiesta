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

public class CatalogPublisherEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/tenants/{tenantId}/catalog/publishers")
            .WithTags("Catalog");

        group.MapGet("/{id}", GetPublisher).WithName("GetCatalogPublisher")
            .Produces<ProblemDetails>(400).Produces<ProblemDetails>(500);

        group.MapGet("/{id}/books", GetPublisherBooks).WithName("GetCatalogPublisherBooks")
            .Produces<ProblemDetails>(400).Produces<ProblemDetails>(500);

        group.MapGet(string.Empty, GetPublishers).WithName("GetCatalogPublishers")
            .Produces<ProblemDetails>(400).Produces<ProblemDetails>(500);
    }

    private static async Task<Results<Ok<PublisherModel>, NotFound, ProblemHttpResult>> GetPublisher(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new PublisherFindOneQuery(tenantId, id))).Result;

        return result.Value == null ? TypedResults.NotFound() : result.IsSuccess
            ? TypedResults.Ok(mapper.Map<Publisher, PublisherModel>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<BookModel>>, NotFound, ProblemHttpResult>> GetPublisherBooks(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new BookFindAllForPublisherQuery(tenantId, id))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Book>, IEnumerable<BookModel>>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<PublisherModel>>, ProblemHttpResult>> GetPublishers(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string tenantId)
    {
        var result = (await mediator.Send(new PublisherFindAllQuery(tenantId))).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Publisher>, IEnumerable<PublisherModel>>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }
}