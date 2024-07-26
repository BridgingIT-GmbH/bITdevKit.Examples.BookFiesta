namespace BridgingIT.DevKit.Examples.BookStore.Organization.Presentation.Web;

using System.Collections.Generic;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookStore.Organization.Application;
using BridgingIT.DevKit.Examples.BookStore.Organization.Domain;
using BridgingIT.DevKit.Presentation.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class OrganizationTenantEndpoints : EndpointsBase
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/tenants")
            .WithTags("Organization");

        group.MapGet("/{id}", async Task<Results<Ok<TenantModel>, NotFound, ProblemHttpResult>> (
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper,
            [FromRoute] string tenantId,
            [FromRoute] string id) =>
        {
            var result = (await mediator.Send(new TenantFindOneQuery(id))).Result;

            return (result.Value == null) ? TypedResults.NotFound() : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Tenant, TenantModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
        }).WithName("GetOrganizationTenant")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet(string.Empty, async Task<Results<Ok<IEnumerable<TenantModel>>, ProblemHttpResult>> (
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper) =>
        {
            var result = (await mediator.Send(new TenantFindAllQuery())).Result;

            return result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Tenant, TenantModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
        }).WithName("GetOrganizationTenants")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost(string.Empty, async Task<Results<Created<TenantModel>, ProblemHttpResult>> (
            [FromServices] IMediator mediator,
            [FromServices] IMapper mapper,
            [FromBody] TenantModel model) =>
        {
            var result = (await mediator.Send(new TenantCreateCommand(model))).Result;

            return result.IsSuccess
                ? TypedResults.Created($"api/tenants/{result.Value.Id}",
                                       mapper.Map<Tenant, TenantModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "),
                                       statusCode: 400);
        }).WithName("CreateOrganizationTenant")
            .Produces<TenantModel>(201)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);
    }
}
