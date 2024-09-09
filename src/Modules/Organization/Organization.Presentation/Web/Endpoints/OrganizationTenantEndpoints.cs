namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Presentation.Web;

using System.Collections.Generic;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Domain;
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
        var group = app.MapGroup("api/organization/tenants")
            .WithTags("Organization");

        group.MapGet("/{id}", GetTenant).WithName("GetOrganizationTenant")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapGet(string.Empty, GetTenants).WithName("GetOrganizationTenants")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        group.MapPost(string.Empty, CreateTenant).WithName("CreateOrganizationTenant")
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(500);

        // TODO: update/delete tenant
    }

    private static async Task<Results<Ok<TenantModel>, NotFound, ProblemHttpResult>> GetTenant(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new TenantFindOneQuery(id))).Result;

        return result.Value == null ? TypedResults.NotFound() : result.IsSuccess
            ? TypedResults.Ok(mapper.Map<Tenant, TenantModel>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<TenantModel>>, ProblemHttpResult>> GetTenants(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper)
    {
        var result = (await mediator.Send(new TenantFindAllQuery())).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Tenant>, IEnumerable<TenantModel>>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Created<TenantModel>, ProblemHttpResult>> CreateTenant(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromBody] TenantModel model)
    {
        var result = (await mediator.Send(new TenantCreateCommand(model))).Result;

        return result.IsSuccess
            ? TypedResults.Created($"api/tenants/{result.Value.Id}",
                                   mapper.Map<Tenant, TenantModel>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "),
                                   statusCode: 400);
    }
}