namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Presentation.Web;

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

/// <summary>
///     Specifies the external API for this module that will be exposed for the outside boundary
/// </summary>
public class OrganizationTenantEndpoints : EndpointsBase
{
    /// <summary>
    ///     Maps the endpoints for the Organization Tenant to the specified route builder.
    /// </summary>
    /// <param name="app">The IEndpointRouteBuilder instance used to define routes.</param>
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/organization/tenants")
            .WithTags("Organization");

        group.MapGet("/{id}", TenantFindOne)
            .WithName("GetOrganizationTenant")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapGet(string.Empty, TenantFindAll)
            .WithName("GetOrganizationTenants")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        group.MapPost(string.Empty, TenantCreate)
            .WithName("CreateOrganizationTenant")
            .ProducesValidationProblem()
            .ProducesProblem(500);
        //.Produces<ProblemDetails>(400)
        //.Produces<ProblemDetails>(500);

        // TODO: update/delete tenant
    }

    private static async Task<Results<Ok<TenantModel>, NotFound, ProblemHttpResult>> TenantFindOne(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromRoute] string id)
    {
        var result = (await mediator.Send(new TenantFindOneQuery(id))).Result;

        return result.IsFailure && result.HasError<NotFoundResultError>()
            ? TypedResults.NotFound()
            : result.IsSuccess
                ? TypedResults.Ok(mapper.Map<Tenant, TenantModel>(result.Value))
                : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Ok<IEnumerable<TenantModel>>, ProblemHttpResult>> TenantFindAll(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper)
    {
        var result = (await mediator.Send(new TenantFindAllQuery())).Result;

        return result.IsSuccess
            ? TypedResults.Ok(mapper.Map<IEnumerable<Tenant>, IEnumerable<TenantModel>>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }

    private static async Task<Results<Created<TenantModel>, ProblemHttpResult>> TenantCreate(
        [FromServices] IMediator mediator,
        [FromServices] IMapper mapper,
        [FromBody] TenantModel model)
    {
        var result = (await mediator.Send(new TenantCreateCommand(model))).Result;

        return result.IsSuccess
            ? TypedResults.Created($"api/tenants/{result.Value.Id}", mapper.Map<Tenant, TenantModel>(result.Value))
            : TypedResults.Problem(result.Messages.ToString(", "), statusCode: 400);
    }
}